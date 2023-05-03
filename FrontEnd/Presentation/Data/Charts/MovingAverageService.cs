using ApplicationModels.Quotes;
using Skender.Stock.Indicators;

namespace Presentation.Data.Charts;

public class MovingAverageService
{
    private readonly ILogger<MovingAverageService> logger;
    private readonly PriceService priceService;
    private List<Quote> Quotes;
    private string lastTicker = string.Empty;

    public MovingAverageService(ILogger<MovingAverageService> logger
        , PriceService priceService)
    {
        this.logger = logger;
        this.priceService = priceService;
        Quotes = new();
    }

    public async Task<List<Quote>> ExecAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return new List<Quote>();
        }
        if (lastTicker.Equals(ticker))
        {
            return Quotes;
        }
        YPrice? yPrices = await priceService.ExecAsync(ticker);
        if (yPrices == null)
        {
            logger.LogInformation($"Could not get price information for {ticker}");
            return new List<Quote>();
        }
        lastTicker = ticker;
        Quotes.Clear();
        Quotes.AddRange(from compressedQuote in yPrices.CompressedQuotes
                        select (Quote)compressedQuote);
        return Quotes;
    }

    public async Task<IEnumerable<SmaResult>?> ExecAsyncSma(string ticker, int period, bool persistedData = true)
    {
        if (priceService == null)
        {
            logger.LogError("Services were not built by CLI!");
            return null;
        }
        await ExecAsync(ticker);
        IEnumerable<SmaResult> results = Quotes.GetSma(period);
        return results;
    }

    public async Task<IEnumerable<EmaResult>?> ExecAsyncEma(string ticker, int period, bool persistedData = true)
    {
        if (priceService == null)
        {
            logger.LogError("Services were not built by CLI!");
            return null;
        }
        if (!lastTicker.Equals(ticker) || !Quotes!.Any() || persistedData != true)
        {
            await ExecAsync(ticker);
        }
        IEnumerable<EmaResult> results = Quotes.GetEma(period);
        return results;
    }
}