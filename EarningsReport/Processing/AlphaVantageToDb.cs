using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement.AlphaVantage;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarningsReport.Processing;

public class AlphaVantageToDb
{
    #region Private Fields

    private readonly IRepository<BalanceSheet> balanceSheetRepository;
    private readonly IRepository<CashFlow> cashFlowRepository;
    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly IRepository<IncomeStatement> incomeStatementRepository;
    private readonly ILogger<AlphaVantageToDb> logger;
    private readonly IRepository<Overview> overViewRepository;

    #endregion Private Fields

    #region Public Constructors

    public AlphaVantageToDb(IRepository<IndexComponent> idxRepository
        , IRepository<EarningsCalendar> ecRepository
        , IRepository<Overview> overViewRepository
        , IRepository<BalanceSheet> balanceSheetRepository
        , IRepository<IncomeStatement> incomeStatementRepository
        , IRepository<CashFlow> cashFlowRepository
        , ILogger<AlphaVantageToDb> logger
        )
    {
        this.balanceSheetRepository = balanceSheetRepository;
        this.cashFlowRepository = cashFlowRepository;
        this.ecRepository = ecRepository;
        this.idxRepository = idxRepository;
        this.incomeStatementRepository = incomeStatementRepository;
        this.logger = logger;
        this.overViewRepository = overViewRepository;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync(Overview overview, BalanceSheet balanceSheet, IncomeStatement incomeStatement, CashFlow cashFlow)
    {
        //1. Delete securities that are not in indexes.
        if (!await RemoveIndexDroppedSecurities())
        {
            return false;
        }
        //2. Delete securities that are to inserted
        if (!await RemoveStatementsToBeRefreshed(overview.Ticker))
        {
            return false;
        }
        //3. Add new records to database
        if (!await AddNewRecordsToDb(overview, balanceSheet, incomeStatement, cashFlow))
        {
            return false;
        }
        //4. Update Earnings Calendar
        bool updateResult = await UpdateEarningsCalendarAsync(overview.Ticker);
        return updateResult;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<bool> AddNewRecordsToDb(Overview overview, BalanceSheet balanceSheet, IncomeStatement incomeStatement, CashFlow cashFlow)
    {
        try
        {
            await balanceSheetRepository.Add(balanceSheet);
            await cashFlowRepository.Add(cashFlow);
            await incomeStatementRepository.Add(incomeStatement);
            await overViewRepository.Add(overview);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to add records in AlphaVantageToDb:AddNewRecordsToDb");
            logger.LogError($"{ex.Message}");
            return false;
        }
        return true;
    }

    private async Task<bool> RemoveIndexDroppedSecurities()
    {
        try
        {
            List<string> tickers = (await idxRepository.FindAll())
                 .Select(x => x.Ticker)
                 .ToList();
            List<string> tickersInFinReport = (await overViewRepository.FindAll())
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            IEnumerable<string> droppedTickers = tickersInFinReport.Except(tickers);
            if (droppedTickers.Any())
            {
                var dropRecords = await overViewRepository.FindAll(x => droppedTickers.Contains(x.Ticker));
                await overViewRepository.Remove(dropRecords);
            }
            tickersInFinReport = (await balanceSheetRepository.FindAll())
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            droppedTickers = tickersInFinReport.Except(tickers);
            if (droppedTickers.Any())
            {
                var dropRecords = await balanceSheetRepository.FindAll(x => droppedTickers.Contains(x.Ticker));
                await balanceSheetRepository.Remove(dropRecords);
            }
            tickersInFinReport = (await incomeStatementRepository.FindAll())
               .Select(x => x.Ticker)
               .Distinct()
               .ToList();
            droppedTickers = tickersInFinReport.Except(tickers);
            if (droppedTickers.Any())
            {
                var dropRecords = await incomeStatementRepository.FindAll(x => droppedTickers.Contains(x.Ticker));
                await incomeStatementRepository.Remove(dropRecords);
            }
            tickersInFinReport = (await cashFlowRepository.FindAll())
               .Select(x => x.Ticker)
               .Distinct()
               .ToList();
            droppedTickers = tickersInFinReport.Except(tickers);
            if (droppedTickers.Any())
            {
                var dropRecords = await cashFlowRepository.FindAll(x => droppedTickers.Contains(x.Ticker));
                await cashFlowRepository.Remove(dropRecords);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop securities in AlphaVantageToDb:RemoveIndexDroppedSecurities");
            logger.LogError($"{ex.Message}");
            return false;
        }
        return true;
    }

    private async Task<bool> RemoveStatementsToBeRefreshed(string ticker)
    {
        try
        {
            var bsRecord = await balanceSheetRepository.FindAll(x => x.Ticker == ticker);
            if (bsRecord != null && bsRecord.Any())
            {
                await balanceSheetRepository.Remove(bsRecord);
            }
            var cfRecord = await cashFlowRepository.FindAll(x => x.Ticker == ticker);
            if (cfRecord != null && cfRecord.Any())
            {
                await cashFlowRepository.Remove(cfRecord);
            }
            var incomeRecord = await incomeStatementRepository.FindAll(x => x.Ticker == ticker);
            if (incomeRecord != null && incomeRecord.Any())
            {
                await incomeStatementRepository.Update(incomeRecord);
            }
            var overviewRecord = await overViewRepository.FindAll(x => x.Ticker == ticker);
            if (overviewRecord != null && overviewRecord.Any())
            {
                await overViewRepository.Remove(overviewRecord);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop aged records in AlphaVantageToDb:RemoveStatementsToBeRefreshed");
            logger.LogError($"{ex.Message}");
            return false;
        }
        return true;
    }

    private async Task<bool> UpdateEarningsCalendarAsync(string ticker)
    {
        //1. Remove aged records
        DateTime oneMonthAgo = DateTime.UtcNow.Date.AddMonths(-1);
        DateTime oneMonthAfter = DateTime.UtcNow.Date.AddMonths(1);
        DateTime today = DateTime.UtcNow.Date;
        try
        {
            IEnumerable<EarningsCalendar> recordsToRemove = await ecRepository.FindAll(x => x.RemoveDate <= oneMonthAgo && x.DataObtained);
            if (recordsToRemove.Any())
            {
                await ecRepository.Remove(recordsToRemove);
            }
            var recordsToUpdate = await ecRepository.FindAll(x => x.Ticker.Equals(ticker));
            if (!recordsToUpdate.Any())
            {
                logger.LogError("Could not find ticker in Earnings calendar");
                return false;
            }
            foreach (var record in recordsToUpdate)
            {
                record.DataObtained = true;
                record.RemoveDate = oneMonthAfter;
                record.EarningsReadDate = today;
            }
            await ecRepository.Update(recordsToUpdate);
        }
        catch (Exception ex)
        {
            logger.LogError("Error while updating EarningsCalendar");
            logger.LogError($"{ex.Message}");
            return false;
        }
        return true;
    }

    #endregion Private Methods
}