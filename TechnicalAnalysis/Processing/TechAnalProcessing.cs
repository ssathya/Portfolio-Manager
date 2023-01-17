using ApplicationModels.Compute;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace TechnicalAnalysis.Processing;

public class TechAnalProcessing
{
    private const int ApproxWorkingDaysInAYear = 252;
    private const int BatchSize = 100;
    private const int DollorVolumeDays = 20;
    private const int MomentumWindow = 125;
    private const int MoneyFlowOffset = 14;
    private readonly IRepository<Compute> computeRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<TechAnalProcessing> logger;
    private readonly IRepository<MomMfDolAvg> summaryRepository;
    private readonly List<decimal> xAxis = new();
    private readonly IRepository<YPrice> yRepository;
    private List<YPrice> yPrices = new();

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

    private static void ComputeMoneyFlow(List<CompressedQuote> yQuotes, Compute momentumsForTicker)
    {
        yQuotes.Sort((a, b) => a.Date.CompareTo(b.Date));
        int iterator = MoneyFlowOffset;
        List<decimal> lows = new(); List<decimal> highs = new(); List<decimal> closes = new();
        List<long> volumes = new();
        while (iterator + MomentumWindow < yQuotes.Count)
        {
            lows = yQuotes.Select(x => x.Low)
                .Skip(iterator - MoneyFlowOffset)
                .Take(MoneyFlowOffset)
                .ToList();
            highs = yQuotes.Select(x => x.High)
                .Skip(iterator - MoneyFlowOffset)
                .Take(MoneyFlowOffset)
                .ToList();
            closes = yQuotes.Select(x => x.ClosingPrice)
                .Skip(iterator - MoneyFlowOffset)
                .Take(MoneyFlowOffset)
                .ToList();
            volumes = yQuotes.Select(x => x.Volume)
                .Skip(iterator - MoneyFlowOffset)
                .Take(MoneyFlowOffset)
                .ToList();
            List<decimal> typicalPrices = new();
            for (int i = 0; i < lows.Count; i++)
            {
                typicalPrices.Add((lows[i] + highs[i] + closes[i]) * volumes[i] / 3);
            }
            decimal positiveMoneyFlow = 0M;
            decimal negativeMoneyFlow = 0M;
            for (int i = 1; i < typicalPrices.Count; i++)
            {
                if (typicalPrices[i] > typicalPrices[i - 1])
                {
                    positiveMoneyFlow += typicalPrices[i];
                }
                else
                {
                    negativeMoneyFlow += typicalPrices[i];
                }
            }
            decimal moneyRatio = positiveMoneyFlow / negativeMoneyFlow;
            decimal moneyRatioPercent = 100 - (100 / (1 + moneyRatio));
            momentumsForTicker.ComputedValues[iterator].MoneyFlow = moneyRatioPercent;
            iterator++;
        }
    }

    private decimal ComputeDollarVolume(YPrice pricesForTicker)
    {
        var twentyDayValue = pricesForTicker.CompressedQuotes
            .OrderBy(r => r.Date)
            .TakeLast(DollorVolumeDays)
            .Select(r => (r.ClosingPrice * r.Volume))
            .Sum();
        return twentyDayValue / DollorVolumeDays;
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
            momentumValues.ComputedValues.Add(new()
            {
                MomentumValue = (decimal)annualizedSlope,
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
                    MoneyFlow = tmpValues.MoneyFlow
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
}