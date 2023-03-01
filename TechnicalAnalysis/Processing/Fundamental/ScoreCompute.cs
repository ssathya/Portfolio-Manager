using ApplicationModels.Compute;
using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using TechnicalAnalysis.Model;

namespace TechnicalAnalysis.Processing.Fundamental;

public class ScoreCompute
{
    #region Private Fields

    private const int OneYearOffSet = 4;
    private readonly IRepository<BalanceSheet> balanceSheetRepository;
    private readonly IRepository<CashFlow> cashFlowRepository;
    private readonly IRepository<IncomeStatement> incomeStatementRepository;
    private readonly ILogger<ScoreCompute> logger;
    private readonly IRepository<Overview> overViewRepository;
    private readonly IRepository<ScoreDetail> scoreDetailRepository;
    private Dictionary<string, int> computedValues = new();

    #endregion Private Fields

    #region Public Constructors

    public ScoreCompute(IRepository<Overview> overViewRepository
        , IRepository<BalanceSheet> balanceSheetRepository
        , IRepository<IncomeStatement> incomeStatementRepository
        , IRepository<CashFlow> cashFlowRepository
        , IRepository<ScoreDetail> scoreDetailRepository
        , ILogger<ScoreCompute> logger)
    {
        this.overViewRepository = overViewRepository;
        this.balanceSheetRepository = balanceSheetRepository;
        this.incomeStatementRepository = incomeStatementRepository;
        this.cashFlowRepository = cashFlowRepository;
        this.scoreDetailRepository = scoreDetailRepository;
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync()
    {
        //1. Read all financial statements
        List<BalanceSheet> balanceSheets = await ReadAllBalanceSheetsAsync();
        List<CashFlow> cashFlows = await ReadAllCashFlowsAsync();
        List<IncomeStatement> incomeStatements = await ReadAllIncomeStatementsAsync();
        if (balanceSheets.Count != cashFlows.Count && balanceSheets.Count != incomeStatements.Count)
        {
            logger.LogInformation("Inconstant values in database");
        }
        List<ScoreDetail> scoreDetail = new();
        foreach (var balanceSheet in balanceSheets)
        {
            if (string.IsNullOrEmpty(balanceSheet.Ticker)) continue;
            var df = new DerivedFinancials();
            PopulateBSValues(balanceSheet, df);
            //Check if there is insufficient values; if yes computed values has a zero for this ticker.
            if (computedValues.ContainsKey(balanceSheet.Ticker)) continue;
            CashFlow? cashFlow = cashFlows.FirstOrDefault(x => x.Ticker == balanceSheet.Ticker);
            if (cashFlow != default)
            {
                PopulateCFValues(cashFlow, df);
            }
            var incomeStatement = incomeStatements.FirstOrDefault(x => x.Ticker == balanceSheet.Ticker);
            if (incomeStatement != default)
            {
                PopulateISValues(incomeStatement, df);
            }
            FixZeroValues(df);
            scoreDetail.Add(ComputePScore(df, balanceSheet));
        }
        scoreDetail.AddRange(HandleSecuritiesWithInsufficientData());
        bool saveResult = await SaveValuesToDb(scoreDetail);
        return saveResult;
    }

    #endregion Public Methods

    #region Private Methods

    private static void FixZeroValues(DerivedFinancials df)
    {
        decimal oneCent = 0.01M;
        df.LongTermDebt = df.LongTermDebt == 0 ? oneCent : df.LongTermDebt;
        df.TotalAssets = df.TotalAssets == 0 ? oneCent : df.TotalAssets;
        df.CurrentAssets = df.CurrentAssets == 0 ? oneCent : df.CurrentAssets;
        df.CurrentLiabilities = df.CurrentLiabilities == 0 ? oneCent : df.CurrentLiabilities;
        df.PyLongTermDebt = df.PyLongTermDebt == 0 ? oneCent : df.PyLongTermDebt;
        df.PyTotalAssets = df.PyTotalAssets == 0 ? oneCent : df.PyTotalAssets;
        df.PyCurrentAssets = df.PyCurrentAssets == 0 ? oneCent : df.PyCurrentAssets;
        df.PyPyTotalAssets = df.PyPyTotalAssets == 0 ? oneCent : df.PyPyTotalAssets;
        df.WaSharesOutstanding = df.WaSharesOutstanding == 0 ? 1 : df.WaSharesOutstanding;
        df.PyWaSharesOutstanding = df.PyWaSharesOutstanding == 0 ? 1 : df.PyWaSharesOutstanding;
        df.CyRevenue = df.CyRevenue == 0 ? oneCent : df.CyRevenue;
        df.PyRevenue = df.PyRevenue == 0 ? oneCent : df.PyRevenue;
        df.CyGrossProfit = df.CyGrossProfit == 0 ? oneCent : df.CyGrossProfit;
        df.PyGrossProfit = df.PyGrossProfit == 0 ? oneCent : df.PyGrossProfit;
        df.CyNetIncome = df.CyNetIncome == 0 ? oneCent : df.CyNetIncome;
        df.PyNetIncome = df.PyNetIncome == 0 ? oneCent : df.PyNetIncome;
    }

    private ScoreDetail ComputePScore(DerivedFinancials df, BalanceSheet balanceSheet)
    {
        var bsLstDate = balanceSheet.AnnualReports.OrderByDescending(r => r.FiscalDateEnding).First();
        computedValues[df.Ticker] = ComputeFScore.ComputeScores(df);
        ScoreDetail sd = new()
        {
            Ticker = df.Ticker,
            PiotroskiComputedValue = computedValues[df.Ticker],
            LastEarningsDate = bsLstDate.FiscalDateEnding.ToUniversalTime(),
            SimFinRating = 0,
            ReturnOnAssets = ComputeFScore.Compute1ReturnOnAssets(df) == 1,
            OperatingCashFlow = ComputeFScore.Compute2OperatingCashFlow(df) == 1,
            IsROABetter = ComputeFScore.Compute3IsROABetter(df) == 1,
            Accruals = ComputeFScore.Compute4Accruals(df) == 1,
            ChangeInLeverage = ComputeFScore.Compute5ChangeInLeverage(df) == 1,
            ChangeInCurrentRatio = ComputeFScore.Compute6ChangeInCurrentRatio(df) == 1,
            ChangeInNumberOfShares = ComputeFScore.Compute7ChangeInNumberOfShares(df) == 1,
            IncreaseGrossMargin = ComputeFScore.Compute8IncreaseGrossMargin(df) == 1,
            AssetTurnoverRatio = ComputeFScore.Compute9AssetTurnoverRatio(df) == 1
        };
        return sd;
    }

    private IEnumerable<ScoreDetail> HandleSecuritiesWithInsufficientData()
    {
        List<ScoreDetail> scoreDetail = new();
        List<string> tickers = new();
        tickers.AddRange(from cv in computedValues
                         where cv.Value == 0
                         select cv.Key);
        foreach (var ticker in tickers)
        {
            scoreDetail.Add(new()
            {
                Ticker = ticker,
                PiotroskiComputedValue = 0,
                LastEarningsDate = new DateTime(1900, 1, 1).ToUniversalTime()
            });
        }
        return scoreDetail;
    }

    private void PopulateBSValues(BalanceSheet balanceSheet, DerivedFinancials df)
    {
        if (balanceSheet.AnnualReports.Count >= 3)
        {
            var bs = balanceSheet.AnnualReports.OrderByDescending(r => r.FiscalDateEnding)
            .ToList();
            df.LongTermDebt = bs[0].LongTermDebt ?? 0;
            df.TotalAssets = bs[0].TotalAssets ?? 0;
            df.CurrentAssets = bs[0].TotalCurrentAssets ?? 0;
            df.CurrentLiabilities = bs[0].TotalCurrentLiabilities ?? 0;
            df.PyLongTermDebt = bs[1].LongTermDebt ?? 0;
            df.PyTotalAssets = bs[1].TotalAssets ?? 0;
            df.PyPyTotalAssets = bs[2].TotalAssets ?? 0;
            df.PyCurrentAssets = bs[1].TotalCurrentAssets ?? 0;
            df.PyCurrentLiabilities = bs[1].TotalCurrentLiabilities ?? 0;
            df.WaSharesOutstanding = bs[0].CommonStockSharesOutstanding ?? 0;
            df.PyWaSharesOutstanding = bs[1].CommonStockSharesOutstanding ?? 0;
            df.Ticker = balanceSheet.Ticker;
            return;
        }
        else if (balanceSheet.QuarterlyReports.Count >= 9)
        {
            var bs = balanceSheet.QuarterlyReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
            df.LongTermDebt = bs[0].LongTermDebt ?? 0;
            df.TotalAssets = bs[0].TotalAssets ?? 0;
            df.CurrentAssets = bs[0].TotalCurrentAssets ?? 0;
            df.CurrentLiabilities = bs[0].TotalCurrentLiabilities ?? 0;
            df.PyLongTermDebt = bs[OneYearOffSet].LongTermDebt ?? 0;
            df.PyTotalAssets = bs[OneYearOffSet].TotalAssets ?? 0;
            df.PyPyTotalAssets = bs[OneYearOffSet + OneYearOffSet].TotalAssets ?? 0;
            df.PyCurrentAssets = bs[OneYearOffSet].TotalCurrentAssets ?? 0;
            df.PyCurrentLiabilities = bs[OneYearOffSet].TotalCurrentLiabilities ?? 0;
            df.WaSharesOutstanding = bs[0].CommonStockSharesOutstanding ?? 0;
            df.PyWaSharesOutstanding = bs[OneYearOffSet].CommonStockSharesOutstanding ?? 0;
            //error in current Quarter. Let us step back by a quarter.
            if (df.WaSharesOutstanding == 0)
            {
                df.WaSharesOutstanding = bs[0 + 1].CommonStockSharesOutstanding ?? 0;
            }
            if (df.PyWaSharesOutstanding == 0)
            {
                df.PyWaSharesOutstanding = bs[OneYearOffSet + 1].CommonStockSharesOutstanding ?? 0;
            }
            return;
        }
        else
        {
            computedValues[balanceSheet.Ticker] = 0;
        }
    }

    private void PopulateCFValues(CashFlow cashFlow, DerivedFinancials df)
    {
        List<CashFlowReport> cfAnnualReports = cashFlow.AnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        List<CashFlowReport> cfQuarterlyReports = cashFlow.QuarterlyReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        if (cfAnnualReports.First().FiscalDateEnding < cfQuarterlyReports.First().FiscalDateEnding)
        {
            //compute TTM values
            string ticker = cashFlow.Ticker;
            UpdateAnnualCashFlowReportsWithTTMValues(cfAnnualReports, cfQuarterlyReports, ticker);
        }

        if (cashFlow.AnnualReports.Count >= 3)
        {
            var cfs = cashFlow.AnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
            df.OperatingCashFlow = cfs[0].OperatingCashflow ?? 0;
            return;
        }
        else if (cashFlow.QuarterlyReports.Count >= 9)
        {
            var cfs = cashFlow.QuarterlyReports.OrderBy(r => r.FiscalDateEnding).ToList();
            df.OperatingCashFlow = (cfs[0].OperatingCashflow ?? 0)
            + (cfs[1].OperatingCashflow ?? 0)
            + (cfs[2].OperatingCashflow ?? 0)
            + (cfs[3].OperatingCashflow ?? 0);
        }
    }

    private void PopulateISValues(IncomeStatement incomeStatement, DerivedFinancials df)
    {
        List<IncomeReport> isAnnualReports = incomeStatement.AnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        List<IncomeReport> isQuarterlyReports = incomeStatement.QuarterlyReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        if (isAnnualReports.First().FiscalDateEnding < isQuarterlyReports.First().FiscalDateEnding)
        {
            //compute TTM values
            string ticker = incomeStatement.Ticker;
            UpdateAnnualIncomeStatementReportsWithTTMValues(isAnnualReports, isQuarterlyReports, ticker);
        }
        if (incomeStatement.AnnualReports.Count >= 3)
        {
            var incStm = incomeStatement.AnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
            df.CyRevenue = incStm[0].TotalRevenue ?? 0;
            df.PyRevenue = incStm[1].TotalRevenue ?? 0;
            df.CyGrossProfit = incStm[0].GrossProfit ?? 0;
            df.PyGrossProfit = incStm[1].GrossProfit ?? 0;
            df.CyNetIncome = incStm[0].NetIncome ?? 0;
            df.PyNetIncome = incStm[1].NetIncome ?? 0;
            return;
        }
        else if (incomeStatement.QuarterlyReports.Count >= 9)
        {
            var incStm = incomeStatement.QuarterlyReports.OrderByDescending(r => r.FiscalDateEnding)
            .ToList();
            df.CyRevenue =
                (incStm[0].TotalRevenue ?? 0)
                + (incStm[1].TotalRevenue ?? 0)
                + (incStm[2].TotalRevenue ?? 0)
                + (incStm[3].TotalRevenue ?? 0);
            df.PyRevenue =
                (incStm[OneYearOffSet + 0].TotalRevenue ?? 0)
                + (incStm[OneYearOffSet + 1].TotalRevenue ?? 0)
                + (incStm[OneYearOffSet + 2].TotalRevenue ?? 0)
                + (incStm[OneYearOffSet + 3].TotalRevenue ?? 0);
            df.CyGrossProfit = (incStm[0].GrossProfit ?? 0)
                + (incStm[1].GrossProfit ?? 0)
                + (incStm[2].GrossProfit ?? 0)
                + (incStm[3].GrossProfit ?? 0);
            df.PyGrossProfit =
                (incStm[OneYearOffSet + 0].GrossProfit ?? 0)
                + (incStm[OneYearOffSet + 1].GrossProfit ?? 0)
                + (incStm[OneYearOffSet + 2].GrossProfit ?? 0)
                + (incStm[OneYearOffSet + 3].GrossProfit ?? 0);
            df.CyNetIncome = (incStm[0].NetIncome ?? 0)
                + (incStm[1].NetIncome ?? 0)
                + (incStm[2].NetIncome ?? 0)
                + (incStm[3].NetIncome ?? 0);
            df.PyNetIncome =
                (incStm[OneYearOffSet + 0].NetIncome ?? 0)
                + (incStm[OneYearOffSet + 1].NetIncome ?? 0)
                + (incStm[OneYearOffSet + 2].NetIncome ?? 0)
                + (incStm[OneYearOffSet + 3].NetIncome ?? 0);
        }
    }

    private async Task<List<BalanceSheet>> ReadAllBalanceSheetsAsync()
    {
        try
        {
            List<BalanceSheet> balanceSheets = (await balanceSheetRepository.FindAll())
                .ToList();
            return balanceSheets;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read balance sheet statements ScoreCompute:ReadAllBalanceSheetsAsync");
            logger.LogError(ex.ToString());
        }
        return new List<BalanceSheet>();
    }

    private async Task<List<CashFlow>> ReadAllCashFlowsAsync()
    {
        try
        {
            List<CashFlow> cashFlows = (await cashFlowRepository.FindAll())
               .ToList();
            return cashFlows;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read balance sheet statements ScoreCompute:ReadAllCashFlowsAsync");
            logger.LogError(ex.ToString());
        }
        return new List<CashFlow>();
    }

    private async Task<List<IncomeStatement>> ReadAllIncomeStatementsAsync()
    {
        try
        {
            List<IncomeStatement> incomeStatements = (await incomeStatementRepository.FindAll())
                .ToList();
            return incomeStatements;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read balance sheet statements ScoreCompute:ReadAllCashFlowsAsync");
            logger.LogError(ex.ToString());
        }
        return new List<IncomeStatement>();
    }

    private async Task<bool> SaveValuesToDb(List<ScoreDetail> scoreDetail)
    {
        try
        {
            //1. Truncate table
            await scoreDetailRepository.Truncate();
            //2. Add all values to database
            await scoreDetailRepository.Add(scoreDetail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update table ScoreDetail in ScoreCompute:SaveValuesToDb");
            logger.LogError(ex.ToString());
            return false;
        }
    }

    private void UpdateAnnualCashFlowReportsWithTTMValues(List<CashFlowReport> cfAnnualReports, List<CashFlowReport> cfQuarterlyReports, string ticker)
    {
        cfAnnualReports = cfAnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        cfQuarterlyReports = cfQuarterlyReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        DateTime lastAnnualReportDt = cfAnnualReports.First().FiscalDateEnding;
        var cfQuarterlyRemove = cfQuarterlyReports.Where(r => r.FiscalDateEnding < lastAnnualReportDt.AddYears(-1)).ToList();
        int quarterReportAdjustments = cfQuarterlyReports.Where(r => r.FiscalDateEnding > lastAnnualReportDt).Count();
        int countRemember = quarterReportAdjustments;
        if (cfQuarterlyRemove.Count < quarterReportAdjustments)
        {
            logger.LogInformation($"Insufficient Quarter yearly reports for {ticker}");
            return;
        }
        CashFlowReport lastCashFlowReport = cfAnnualReports.First();

        while (quarterReportAdjustments > 0)
        {
            lastCashFlowReport.OperatingCashflow += cfQuarterlyReports[quarterReportAdjustments - 1].OperatingCashflow;
            //Add additional attributes if needed
            quarterReportAdjustments--;
        }

        while (countRemember > 0)
        {
            lastCashFlowReport.OperatingCashflow -= cfQuarterlyRemove[countRemember - 1].OperatingCashflow;
            //Add additional attributes if needed
            countRemember--;
        }
    }

    private void UpdateAnnualIncomeStatementReportsWithTTMValues(List<IncomeReport> isAnnualReports, List<IncomeReport> isQuarterlyReports, string ticker)
    {
        isAnnualReports = isAnnualReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        isQuarterlyReports = isQuarterlyReports.OrderByDescending(r => r.FiscalDateEnding).ToList();
        DateTime lasAnnualReportDt = isAnnualReports.First().FiscalDateEnding;
        List<IncomeReport> isQuarterlyRemove = isQuarterlyReports.Where(r => r.FiscalDateEnding < lasAnnualReportDt).ToList();
        int quarterReportAdjustments = isQuarterlyReports.Where(r => r.FiscalDateEnding > lasAnnualReportDt).Count();
        int countRemember = quarterReportAdjustments;
        if (isQuarterlyRemove.Count < quarterReportAdjustments)
        {
            logger.LogInformation($"Insufficient Quarter yearly reports for {ticker}");
            return;
        }
        IncomeReport lastIncomeReport = isAnnualReports.First();
        while (quarterReportAdjustments > 0)
        {
            lastIncomeReport.TotalRevenue += isQuarterlyReports[quarterReportAdjustments - 1].TotalRevenue;
            lastIncomeReport.GrossProfit += isQuarterlyReports[quarterReportAdjustments - 1].GrossProfit;
            lastIncomeReport.NetIncome += isQuarterlyReports[quarterReportAdjustments - 1].NetIncome;
            //Add additional attributes if needed
            quarterReportAdjustments--;
        }
        while (countRemember > 0)
        {
            lastIncomeReport.TotalRevenue -= isQuarterlyRemove[countRemember - 1].TotalRevenue;
            lastIncomeReport.GrossProfit -= isQuarterlyRemove[countRemember - 1].GrossProfit;
            lastIncomeReport.NetIncome -= isQuarterlyRemove[countRemember - 1].NetIncome;
            //Add additional attributes if needed
            countRemember--;
        }
    }

    #endregion Private Methods
}