using ApplicationModels.Compute;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using Skender.Stock.Indicators;

namespace TechnicalAnalysis.Processing;

public class TechAnalProcessing
{
    #region Private Fields

    private const int ApproximateWorkingDaysInAYear = 41;
    private const int BatchSize = 100;
    private const int DolorVolumeDays = 20;
    private const int MomentumWindow = 125;
    private const int MoneyFlowOffset = 14;
    private const int SmaPeriods = 30;
    private readonly IRepository<Compute> computeRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<TechAnalProcessing> logger;
    private readonly IRepository<MomMfDolAvg> summaryRepository;
    private readonly List<decimal> xAxis = new();
    private readonly IRepository<YPrice> yRepository;
    private List<YPrice> yPrices = new();

    #endregion Private Fields

    #region Public Constructors

    public TechAnalProcessing(IRepository<YPrice> yRepository
        , IRepository<IndexComponent> idxRepository
        , IRepository<Compute> computeRepository
        , IRepository<MomMfDolAvg> summaryRepository
        , ILogger<TechAnalProcessing> logger)
    {
        this.computeRepository = computeRepository;
        this.summaryRepository = summaryRepository;
        this.idxRepository = idxRepository;
        this.logger = logger;
        this.yRepository = yRepository;
        for (int i = 1; i <= MomentumWindow; i++)
        {
            xAxis.Add(i);
        }
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync()
    {
        List<string> tickers = await ObtainTickersToProcessAsync();
        if (!tickers.Any())
        {
            return false;
        }
        await ComputeMomentumAndMoneyFlow(tickers);

        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private static void ComputeMoneyFlow(List<CompressedQuote> yQuotes, Compute momentumsForTicker)
    {
        var quotes = from yQuote in yQuotes
                     select (Quote)yQuote;
        IEnumerable<MfiResult> mfiResults = quotes.GetMfi(MoneyFlowOffset);
        foreach (var cv in momentumsForTicker.ComputedValues)
        {
            var mfiResult = mfiResults.FirstOrDefault(r => r.Date.Equals(cv.ReportingDate));
            if (mfiResult != null)
            {
                cv.MoneyFlow = (decimal)(mfiResult.Mfi ?? 0.0);
            }
        }
        return;
    }

    private decimal ComputeDollarVolume(YPrice pricesForTicker)
    {
        var twentyDayValue = pricesForTicker.CompressedQuotes
            .OrderBy(r => r.Date)
            .TakeLast(DolorVolumeDays)
            .Select(r => (r.ClosingPrice * r.Volume))
            .Sum();
        return twentyDayValue / DolorVolumeDays;
    }

    private Compute ComputeMomentum(List<CompressedQuote> yQuotes, string ticker)
    {
        Compute momentumValues = new();
        if (yQuotes.Count < MomentumWindow)
        {
            return momentumValues;
        }

        momentumValues.Ticker = ticker;
        yQuotes.Sort((a, b) => a.Date.CompareTo(b.Date));

        int iterator = 0;

        while (iterator + MomentumWindow < yQuotes.Count)
        {
            var reportingDt = yQuotes[iterator + MomentumWindow].Date;
            //Why did I comment out momentum compute?
            //Checked the value of rSquared and it is closer to zero rather than closer than 1
            //rSquared gives the confidence level. The algorithm is not confident. Then why use it
            //Could have removed the entire section but leaving room for future change of mind.
            /*
            var closingPrices = yQuotes
                .Skip(iterator++)
                .Take(MomentumWindow)
                .Select(x => (Decimal)(Math.Log((double)x.ClosingPrice)))
                .ToArray();
            (decimal rSquared, decimal yIntercept, decimal slope) =
                LinearRegression.LinRegression(xAxis.ToArray(), closingPrices);
            var annualizedSlope = (Math.Pow(Math.Exp((double)slope), ApproxWorkingDaysInAYear) - 1) * 100;
            //Adjust for fitness
            annualizedSlope *= (double)rSquared;
            */
            iterator++;
            momentumValues.ComputedValues.Add(new()
            {
                //MomentumValue = (decimal)annualizedSlope,
                ReportingDate = reportingDt.ToUniversalTime()
            });
        }
        return momentumValues;
    }

    private async Task ComputeMomentumAndMoneyFlow(List<string> tickers)
    {
        await computeRepository.Truncate();
        await summaryRepository.Truncate();
        List<Compute> momentum = new();
        List<MomMfDolAvg> momMfDolAvgs = new();
        int counter = 0;
        foreach (var ticker in tickers)
        {
            List<CompressedQuote> yQuotes = await ObtainQuotesForTicker(ticker);
            if (yQuotes == null || yQuotes.Count == 0)
            {
                logger.LogInformation($"Could not prices for {ticker}");
                continue;
            }
            Compute momentumsForTicker = ComputeMomentum(yQuotes, ticker);
            ComputeRocResults(yQuotes, momentumsForTicker);
            if (momentumsForTicker != null && !string.IsNullOrEmpty(momentumsForTicker.Ticker))
            {
                ComputeMoneyFlow(yQuotes, momentumsForTicker);
                var pricesForTicker = yPrices.First(r => r.Ticker == ticker);
                decimal dollarVolume = 0;
                if (pricesForTicker != null)
                {
                    dollarVolume = ComputeDollarVolume(pricesForTicker);
                }
                momentum.Add(momentumsForTicker);
                ComputedValues tmpValues = momentumsForTicker.ComputedValues.OrderBy(r => r.ReportingDate).Last();
                momMfDolAvgs.Add(new()
                {
                    Ticker = ticker,
                    DollarVolume = dollarVolume,
                    Momentum = tmpValues.MomentumValue,
                    MoneyFlow = tmpValues.MoneyFlow,
                    SmA = tmpValues.Sma
                });
                counter++;
            }
            else
            {
                logger.LogInformation($"Could not compute momentum for {ticker}");
            }
            if (counter % BatchSize == 0)
            {
                await SaveValuesToDatabase(momentum);
                await SaveValuesToDatabase(momMfDolAvgs);
                momentum.Clear();
                momMfDolAvgs.Clear();
            }
        }
        if (momentum.Any())
        {
            await SaveValuesToDatabase(momentum);
            await SaveValuesToDatabase(momMfDolAvgs);
        }
    }

    private void ComputeRocResults(List<CompressedQuote> yQuotes, Compute momentumsForTicker)
    {
        var quotes = from yQuote in yQuotes
                     select (Quote)yQuote;
        var results = quotes.GetRoc(MomentumWindow, SmaPeriods);
        List<ComputedValues> computedValues = momentumsForTicker.ComputedValues;
        if (results.Any())
        {
            foreach (var cv in computedValues)
            {
                RocResult? result = results.FirstOrDefault(x => x.Date.Equals(cv.ReportingDate));
                if (result != null)
                {
                    cv.Roc = (decimal)(result.Roc ?? 0);
                    cv.Sma = (decimal)(result.RocSma ?? 0);
                    cv.MomentumValue = (decimal)(result.Roc ?? 0.0);
                }
            }
        }
    }

    private async Task<List<CompressedQuote>> ObtainQuotesForTicker(string ticker)
    {
        try
        {
            if (yPrices.Count == 0)
            {
                yPrices = (await yRepository.FindAll())
                    .ToList();
            }
            YPrice? yPrice = yPrices.FindAll(x => x.Ticker == ticker)
                .FirstOrDefault();
            if (yPrice == null)
            {
                return new List<CompressedQuote>();
            }
            return yPrice.CompressedQuotes;
        }
        catch (Exception ex)
        {
            logger.LogError("Exception while retrieving Quotes ObtainQuotesForTicker");
            logger.LogError($"{ex.Message}");
            return new List<CompressedQuote>();
        }
    }

    private async Task<List<string>> ObtainTickersToProcessAsync()
    {
        try
        {
            var tickers = (await idxRepository.FindAll())
                .OrderBy(x => x.Ticker)
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            return tickers;
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception while getting all tickers to process ObtainTickersToProcessAsync");
            logger.LogError($"{ex.Message}");
            return new List<string>();
        }
    }

    private async Task SaveValuesToDatabase(List<MomMfDolAvg> momMfDolAvgs)
    {
        try
        {
            await summaryRepository.Add(momMfDolAvgs);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database with new values SaveValuesToDatabase");
            logger.LogError($"{ex.Message}");
        }
    }

    private async Task SaveValuesToDatabase(List<Compute> momentum)
    {
        try
        {
            await computeRepository.Add(momentum);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database with new values SaveValuesToDatabase");
            logger.LogError($"{ex.Message}");
        }
    }

    #endregion Private Methods
}