using AppCommon.CacheHandler;
using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarningsReport.Processing;

public class AlphaVantageReports
{
    #region Private Fields

    private const int MaxNumberOfCalls = 1;
    private const string Vendor = "Alphavantage";
    private readonly IConfiguration configuration;
    private readonly IRepository<EarningsCalendar> ecRepository;

    private readonly Dictionary<FilingNames, string> filings = new()
    {
        { FilingNames.BalanceSheet, "BALANCE_SHEET" },
        { FilingNames.Cashflow, "CASH_FLOW" },
        { FilingNames.Income, "INCOME_STATEMENT" },
        { FilingNames.Overview, "OVERVIEW" }
    };

    private readonly IHandleCache handleCache;
    private readonly ILogger<AlphaVantageReports> logger;

    #endregion Private Fields

    #region Public Constructors

    public AlphaVantageReports(IRepository<EarningsCalendar> ecRepository
        , ILogger<AlphaVantageReports> logger
        , IHandleCache handleCache
        , IConfiguration configuration)
    {
        this.ecRepository = ecRepository;
        this.logger = logger;
        this.handleCache = handleCache;
        this.configuration = configuration;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<(Overview, BalanceSheet, IncomeStatement, CashFlow)> ExecAsync()
    {
        List<EarningsCalendar> earningsCalendars = await GetFirmsToObtainEarningsReportAsync();
        if (earningsCalendars == null || earningsCalendars.Count == 0)
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        var earningsCalendar = earningsCalendars.First();
        Overview? overview = await FetchDataFromAlpahVantageAsync<Overview?>(earningsCalendar.Ticker, FilingNames.Overview);
        if (overview == null || string.IsNullOrEmpty(overview.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixDatesInOverview(overview, earningsCalendar);
        BalanceSheet? balanceSheet = await FetchDataFromAlpahVantageAsync<BalanceSheet?>(earningsCalendar.Ticker, FilingNames.BalanceSheet);
        if (balanceSheet == null || string.IsNullOrEmpty(balanceSheet.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixBalanceSheetDates(balanceSheet);
        IncomeStatement? incomeStatement = await FetchDataFromAlpahVantageAsync<IncomeStatement?>(earningsCalendar.Ticker, FilingNames.Income);
        if (incomeStatement == null || string.IsNullOrEmpty(incomeStatement.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixIncomeStatementDates(incomeStatement);
        CashFlow? cashFlow = await FetchDataFromAlpahVantageAsync<CashFlow?>(earningsCalendar.Ticker, FilingNames.Cashflow);
        if (cashFlow == null || string.IsNullOrEmpty(cashFlow.Ticker))
        {
            return (new Overview(), new BalanceSheet(), new IncomeStatement(), new CashFlow());
        }
        FixCashFlowStatementDates(cashFlow);
        return (overview, balanceSheet, incomeStatement, cashFlow);
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<T?> FetchDataFromAlpahVantageAsync<T>(string symbol, FilingNames filingNames)
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
        try
        {
            var ecRecords = (await ecRepository.FindAll(x => x.DataObtained == false))
                .OrderBy(x => Math.Min(x.VendorEarningsDate.Ticks, x.EarningsDateYahoo.Ticks))
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

    #endregion Private Methods
}