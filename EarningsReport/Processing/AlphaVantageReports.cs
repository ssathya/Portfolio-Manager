using AppCommon.CacheHandler;
using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement.AlphaVantage;
using ApplicationModels.Indexes;
using ApplicationModels.ViewModel;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarningsReport.Processing;

public class AlphaVantageReports
{
    private const int MaxNumberOfCalls = 1;
    private const string Vendor = "Alphavantage";
    private readonly IConfiguration configuration;
    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly IRepository<BalanceSheet> balanceSheetRepository;
    private readonly IRepository<IndexComponent> indexComponentRepository;

    private readonly Dictionary<FilingNames, string> filings = new()
    {
        { FilingNames.BalanceSheet, "BALANCE_SHEET" },
        { FilingNames.Cashflow, "CASH_FLOW" },
        { FilingNames.Income, "INCOME_STATEMENT" },
        { FilingNames.Overview, "OVERVIEW" }
    };

    private readonly IHandleCache handleCache;
    private readonly ILogger<AlphaVantageReports> logger;

    public AlphaVantageReports(IRepository<EarningsCalendar> ecRepository
        , IRepository<BalanceSheet> balanceSheetRepository
        , IRepository<IndexComponent> indexComponentRepository
        , ILogger<AlphaVantageReports> logger
        , IHandleCache handleCache
        , IConfiguration configuration)
    {
        this.ecRepository = ecRepository;
        this.balanceSheetRepository = balanceSheetRepository;
        this.indexComponentRepository = indexComponentRepository;
        this.logger = logger;
        this.handleCache = handleCache;
        this.configuration = configuration;
    }

    public async Task<(Overview, BalanceSheet, IncomeStatement, CashFlow)> ExecAsync()
    {
        List<EarningsCalendar> earningsCalendars = await GetFirmsToObtainEarningsReportAsync();
        if (earningsCalendars == null || earningsCalendars.Count == 0)
        {
            earningsCalendars ??= new();
            await PopulateEarningsCalendarWithMissedValues(earningsCalendars);
        }
        var earningsCalendar = earningsCalendars.First();
        Overview? overview = await FetchDataFromAlpahaVantageAsync<Overview?>(earningsCalendar.Ticker, FilingNames.Overview);
        if (overview == null || string.IsNullOrEmpty(overview.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixDatesInOverview(overview, earningsCalendar);
        BalanceSheet? balanceSheet = await FetchDataFromAlpahaVantageAsync<BalanceSheet?>(earningsCalendar.Ticker, FilingNames.BalanceSheet);
        if (balanceSheet == null || string.IsNullOrEmpty(balanceSheet.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixBalanceSheetDates(balanceSheet);
        IncomeStatement? incomeStatement = await FetchDataFromAlpahaVantageAsync<IncomeStatement?>(earningsCalendar.Ticker, FilingNames.Income);
        if (incomeStatement == null || string.IsNullOrEmpty(incomeStatement.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixIncomeStatementDates(incomeStatement);
        CashFlow? cashFlow = await FetchDataFromAlpahaVantageAsync<CashFlow?>(earningsCalendar.Ticker, FilingNames.Cashflow);
        if (cashFlow == null || string.IsNullOrEmpty(cashFlow.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixCashFlowStatementDates(cashFlow);
        return (overview, balanceSheet, incomeStatement, cashFlow);
    }

    private async Task PopulateEarningsCalendarWithMissedValues(List<EarningsCalendar> earningsCalendars)
    {
        var rnd = new Random((int)DateTime.Now.Ticks % 500);
        List<string> indexTickers = (await indexComponentRepository.FindAll())
            .Select(x => x.Ticker).ToList();
        if (indexTickers.Count == 0)
        { return; }
        List<BalanceSheet> balanceSheets = (await balanceSheetRepository.FindAll()).ToList();
        List<string> earningsCalendarsReadTickers = (await ecRepository.FindAll(x => x.DataObtained == true))
            .Select(x => x.Ticker)
            .ToList();

        List<string> processedTickers = balanceSheets
            .Select(x => x.Ticker).ToList();
        var remainingTickers = indexTickers.Except(processedTickers)
            .ToList();
        if (remainingTickers.Any())
        {
            earningsCalendars.Add(new EarningsCalendar
            {
                DataObtained = false,
                EarningsDateYahoo = new DateTime(2100, 1, 1).ToUniversalTime(),
                VendorEarningsDate = DateTime.Now.Date.ToUniversalTime(),
                EarningsReadDate = DateTime.Now.Date.ToUniversalTime(),
                Ticker = remainingTickers.OrderBy(x => rnd.Next())
                .First() ?? string.Empty
            });
            return;
        }

        string ticker = string.Empty;
        DateTime lastReportedDate = new DateTime(2100, 1, 1);
        foreach (var balanceSheet in balanceSheets)
        {
            List<DateTime> dateTimes = new()
            {
                balanceSheet.AnnualReports.Max(x => x.FiscalDateEnding),
                balanceSheet.QuarterlyReports.Max(x => x.FiscalDateEnding)
            };
            if (lastReportedDate > dateTimes.Max()
                && !earningsCalendarsReadTickers.Contains(balanceSheet.Ticker))
            {
                lastReportedDate = dateTimes.Max();
                ticker = balanceSheet.Ticker;
            }
        }
        if (!string.IsNullOrEmpty(ticker))
        {
            earningsCalendars.Add(new EarningsCalendar
            {
                DataObtained = false,
                EarningsDateYahoo = new DateTime(2100, 1, 1).ToUniversalTime(),
                VendorEarningsDate = DateTime.Now.Date.ToUniversalTime(),
                EarningsReadDate = DateTime.Now.Date.ToUniversalTime(),
                Ticker = ticker
            });
        }

        return;
    }

    private async Task<T?> FetchDataFromAlpahaVantageAsync<T>(string symbol, FilingNames filingNames)
    {
        try
        {
            var urlToUse = configuration[Vendor];
            if (string.IsNullOrEmpty(urlToUse))
            {
                logger.LogCritical("Configuration error");
                return default;
            }
            urlToUse = urlToUse.Replace(@"{symbol}", symbol)
                .Replace(@"{operation}", filings[filingNames]);
            T? returnValue = await handleCache.GetAsync<T>(urlToUse, CacheDuration.Days, 30, false, @"\s\""None\""|\s\""-\""");
            return returnValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error occurred while trying to get data for {symbol} from Alphavantage");
            return default;
        }
    }

    private static void FixBalanceSheetDates(BalanceSheet balanceSheet)
    {
        foreach (var report in balanceSheet.AnnualReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
        foreach (var report in balanceSheet.QuarterlyReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
    }

    private static void FixCashFlowStatementDates(CashFlow cashFlow)
    {
        foreach (var report in cashFlow.AnnualReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
        foreach (var report in cashFlow.QuarterlyReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
    }

    private static void FixDatesInOverview(Overview overview, EarningsCalendar earningsCalendar)
    {
        var defaultDate = new DateTime(1900, 1, 1).ToUniversalTime();
        if (overview.LatestQuarter == null)
        {
            overview.LatestQuarter = earningsCalendar.VendorEarningsDate.ToUniversalTime();
        }
        else
        {
            overview.LatestQuarter = overview.LatestQuarter.Value.ToUniversalTime();
        }
        if (overview.DividendDate != null)
        {
            overview.DividendDate = overview.DividendDate.Value.ToUniversalTime();
        }
        else
        {
            overview.DividendDate = defaultDate;
        }
        if (overview.ExDividendDate != null)
        {
            overview.ExDividendDate = overview.ExDividendDate.Value.ToUniversalTime();
        }
        else
        {
            overview.ExDividendDate = defaultDate;
        }
        if (overview.LastSplitDate != null)
        {
            overview.LastSplitDate = overview.LastSplitDate.Value.ToUniversalTime();
        }
        else
        {
            overview.LastSplitDate = defaultDate;
        }
    }

    private static void FixIncomeStatementDates(IncomeStatement incomeStatement)
    {
        foreach (var report in incomeStatement.AnnualReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
        foreach (var report in incomeStatement.QuarterlyReports)
        {
            report.FiscalDateEnding = report.FiscalDateEnding.ToUniversalTime();
        }
    }

    private async Task<List<EarningsCalendar>> GetFirmsToObtainEarningsReportAsync()
    {
        DateTime defaultDt = DateTime.Now.AddYears(-2).Date.ToUniversalTime();
        DateTime changeDefaultDt = new DateTime(2100, 1, 1).ToUniversalTime();
        try
        {
            var ecRecords = (await ecRepository.FindAll(x => x.DataObtained == false))
                .ToList();
            foreach (var record in ecRecords)
            {
                if (record.VendorEarningsDate < defaultDt)
                {
                    record.VendorEarningsDate = changeDefaultDt;
                }
                if (record.EarningsDateYahoo < defaultDt)
                {
                    record.EarningsDateYahoo = changeDefaultDt;
                }
            }
            ecRecords = ecRecords
                .OrderBy(x => x.VendorEarningsDate)
                .ThenBy(x => x.EarningsDateYahoo)
                .Take(MaxNumberOfCalls)
                .ToList();
            return ecRecords;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error while reading database AlphaVantageReports:GetFirmsToObtainEarningsReport");
            logger.LogError($"{ex.Message}", ex);
            return new List<EarningsCalendar>();
        }
    }
}