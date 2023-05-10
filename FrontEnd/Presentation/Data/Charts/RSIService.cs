using ApplicationModels.Quotes;
using Skender.Stock.Indicators;

namespace Presentation.Data.Charts;

public class RSIService
{
    private readonly ILogger<RSIService> logger;
    private readonly PriceService priceService;
    private string lastTicker = string.Empty;
    private List<Quote> Quotes;

    public RSIService(ILogger<RSIService> logger
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

    public async Task<IEnumerable<RsiResult>> ExecAsync(string ticker, int lookbackPeriods)
    {
        var quotes = await ExecAsync(ticker);
        IEnumerable<RsiResult> result = quotes.GetRsi(lookbackPeriods);
        return result;
    }
}