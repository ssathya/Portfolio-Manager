using ApplicationModels.Quotes;
using ApplicationModels.ViewModel;
using ApplicationModels.Views;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Data.Charts;

public class ComputeLinReg
{
    private readonly ILogger<ComputeLinReg> logger;
    private readonly PriceService priceService;
    private readonly SecurityWithPScoresService securityWithPScoresService;
    private Dictionary<string, List<Quote>> tickerQuotes = new();
    private static List<SecurityDetails> securityDetails = new();
    private static DateTimeOffset? createdTime;
    private static TimeSpan expiresAfter = TimeSpan.FromHours(5);

    public ComputeLinReg(ILogger<ComputeLinReg> logger
        , PriceService priceService
        , SecurityWithPScoresService securityWithPScoresService)
    {
        this.logger = logger;
        this.priceService = priceService;
        this.securityWithPScoresService = securityWithPScoresService;
    }

    public async Task<List<SecurityDetails>> ExecAsync()
    {
        if (securityDetails.Any() && DateTimeOffset.UtcNow - createdTime <= expiresAfter)
        {
            return securityDetails;
        }
        securityDetails.Clear();
        List<SecurityWithPScore> pScores = await securityWithPScoresService.ExecAsync();
        List<YPrice> yPrices = await priceService.ExecAsync() ?? new List<YPrice>();
        foreach (var p in yPrices)
        {
            List<Quote> quotes = (from x in p.CompressedQuotes
                                  select (Quote)x)
                          .ToList();
            tickerQuotes.Add(p.Ticker, quotes);
        }
        foreach (var x in tickerQuotes)
        {
            var ticker = x.Key;
            var pScore = pScores.Where(x => x.Ticker.Equals(ticker))
                .FirstOrDefault();
            if (pScore == default)
            {
                logger.LogInformation($"Could not find {ticker} in Security With PScore");
                continue;
            }
            List<Quote> quotes = x.Value;
            foreach (var quote in quotes)
            {
                quote.Open = (decimal)Math.Log((double)quote.Open);
                quote.High = (decimal)Math.Log((double)quote.High);
                quote.Low = (decimal)Math.Log((double)quote.Low);
                quote.Close = (decimal)Math.Log((double)quote.Close);
            }
            int lookbackPeriods = quotes.Count;
            IEnumerable<SlopeResult> results = quotes.GetSlope(lookbackPeriods);
            SlopeResult result = results.RemoveWarmupPeriods().Last();
            var annualizedSlope = (Math.Pow(Math.Exp(result.Slope ?? 0), lookbackPeriods) - 1) * 100;
            var score = annualizedSlope * result.RSquared;
            securityDetails.Add(pScore);
            securityDetails.Last().Momentum = (decimal)(score ?? 0);
        }
        createdTime = DateTimeOffset.UtcNow;
        return securityDetails;
    }
}