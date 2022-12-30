using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarningsReport.Processing;

public class FinStatementsToDb
{
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly IRepository<FinStatements> finStatementRepository;
    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly ILogger<FinStatementsToDb> logger;

    public FinStatementsToDb(IRepository<IndexComponent> idxRepository
        , IRepository<FinStatements> finStatementRepository
        , IRepository<EarningsCalendar> ecRepository
        , ILogger<FinStatementsToDb> logger)
    {
        this.idxRepository = idxRepository;
        this.finStatementRepository = finStatementRepository;
        this.ecRepository = ecRepository;
        this.logger = logger;
    }

    public async Task<bool> ExcecAsync(List<FinStatements> finStatements)
    {
        //1. Delete securities that are not in indexes.
        if (!await RemoveIndexDroppedSecurities())
        {
            return false;
        }
        //2. Delete securities that are to inserted
        if (!await RemoveStatementsToBeRefreshed(finStatements))
        {
            return false;
        }
        //3. Add records to database
        if (!await AddNewRecordsToDb(finStatements))
        {
            return false;
        }
        bool updateResult = await UpdateEarningsCalendarAsync(finStatements);
        return true;
    }

    private async Task<bool> UpdateEarningsCalendarAsync(List<FinStatements> finStatements)
    {
        //1. Remove aged records
        DateTime oneMonthAgo = DateTime.UtcNow.Date.AddMonths(-1);
        DateTime oneMonthAfter = DateTime.UtcNow.Date.AddMonths(1);
        DateTime today = DateTime.UtcNow.Date;
        IEnumerable<EarningsCalendar> recordsToRemove = await ecRepository.FindAll(x => x.RemoveDate <= oneMonthAgo && x.DataObtained);
        if (recordsToRemove.Any())
        {
            await ecRepository.Remove(recordsToRemove);
        }
        //2. Update processed tickers.
        List<string> processedTickers = finStatements.Select(x => x.Ticker)
            .Distinct()
            .ToList();
        var recordsToUpdate = await ecRepository.FindAll(x => processedTickers.Contains(x.Ticker));
        if (recordsToUpdate == null || recordsToUpdate.Count() == 0)
        {
            return false;
        }
        foreach (var record in recordsToUpdate)
        {
            record.DataObtained = true;
            record.RemoveDate = oneMonthAfter;
            record.EarningsReadDate = today;
        }
        try
        {
            await ecRepository.Update(recordsToUpdate);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error while updating EarningsCalendar");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> AddNewRecordsToDb(List<FinStatements> finStatements)
    {
        try
        {
            await finStatementRepository.Add(finStatements);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to add records FinStatementsToDb:AddNewRecordsToDb");
            logger.LogError($"{ex.Message}");
            return false;
        }
        throw new NotImplementedException();
    }

    private async Task<bool> RemoveStatementsToBeRefreshed(List<FinStatements> finStatements)
    {
        List<string> tickersToRemove = finStatements.Select(x => x.Ticker)
            .Distinct()
            .ToList();
        try
        {
            foreach (var droppedTicker in tickersToRemove)
            {
                var dropRecords = await finStatementRepository.FindAll(x => x.Ticker == droppedTicker);
                if (dropRecords.Any())
                {
                    await finStatementRepository.Remove(dropRecords);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop securities FinStatementsToDb:RemoveStatementsToBeRefreshed");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> RemoveIndexDroppedSecurities()
    {
        try
        {
            List<string> tickers = (await idxRepository.FindAll())
                .Select(x => x.Ticker)
                .ToList();
            List<string> tickersInReport = (await finStatementRepository.FindAll())
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            IEnumerable<string> droppedTickers = tickersInReport.Except(tickers);
            foreach (var droppedTicker in droppedTickers)
            {
                var dropRecords = await finStatementRepository.FindAll(x => x.Ticker == droppedTicker);
                await finStatementRepository.Remove(dropRecords);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop securities that are not in index FinStatementsToDb:RemoveIndexDroppedSecurities");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }
}