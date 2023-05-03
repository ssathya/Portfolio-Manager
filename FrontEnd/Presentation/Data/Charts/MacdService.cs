using ApplicationModels.Quotes;
using Skender.Stock.Indicators;

namespace Presentation.Data.Charts;

public class MacdService
{
    private readonly ILogger<MacdService> logger;
    private readonly PriceService priceService;
    private List<Quote> Quotes;
    private string lastTicker = string.Empty;

    public MacdService(ILogger<MacdService> logger
        , PriceService priceService)
    {
        this.logger = logger;
        this.priceService = priceService;
        Quotes = new();
    }

    public async Task<List<Quote>> ExecAsync(string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
        {
            return new List<Quote>();
        }
        if (lastTicker.Equals(ticker))
        {
            return Quotes;
        }
        YPrice? yPrice = await priceService.ExecAsync(ticker);
        if (yPrice == null)
        {
            logger.LogInformation($"Could not get price information for ticker {ticker}");
            return new List<Quote>();
        }
        lastTicker = ticker;
        Quotes.Clear();
        Quotes.AddRange(from CompressedQuote in yPrice.CompressedQuotes
                        select (Quote)CompressedQuote);
        return Quotes;
    }

    public async Task<IEnumerable<MacdResult>> ExecAsyncMacd(string ticker, int fastPeriod, int slowPeriod, int signalPeriod)
    {
        var quotes = await ExecAsync(ticker);
        IEnumerable<MacdResult> results = quotes.GetMacd(fastPeriod, slowPeriod, signalPeriod);
        return results;
    }
}