using AppCommon.CacheHandler;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using System.Diagnostics;
using System.Globalization;

namespace QuotesManager.Processing;

public class GetValuesFromYahoo
{
    #region Private Fields

    private const string Vendor = "Yahoo";
    private static ManualResetEvent mre = new ManualResetEvent(false);
    private readonly IConfiguration configuration;
    private readonly int end;
    private readonly IHandleCache handleCache;
    private readonly ILogger<GetValuesFromYahoo> logger;
    private readonly IRepository<IndexComponent> repository;
    private readonly int start;

    #endregion Private Fields

    #region Public Constructors

    public GetValuesFromYahoo(IConfiguration configuration
        , ILogger<GetValuesFromYahoo> logger
        , IHandleCache handleCache
        , IRepository<IndexComponent> repository)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.handleCache = handleCache;
        this.repository = repository;
        DateTime endDate = DateTime.Now.Date.AddDays(-1);
        DateTime startDate = endDate.AddMonths(-12);
        DateTime unixDateStart = new(1970, 1, 1);
        start = (int)startDate.Subtract(unixDateStart).TotalSeconds;
        end = (int)endDate.Subtract(unixDateStart).TotalSeconds;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync()
    {
        List<string>? tickers = await ObtainTickersToProcess();
        if (tickers == null || tickers.Count == 0)
        {
            return false;
        }
        Stopwatch stopWatch = new();
        List<YahooQuote> yahooQuotes = new();
        stopWatch.Reset();
        stopWatch.Start();
        for (int i = 0; i < tickers.Count; i++)
        {
            string? ticker = tickers[i];
            yahooQuotes.AddRange(await GetHistoricPricesForTicker(ticker));
            if (i % 10 == 0 && i != 0)
            {
                logger.LogInformation($"Last ticker processed {ticker}");
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                int timeTaken = ts.Nanoseconds;
                int remainingTime = 7000 - timeTaken;
                if (remainingTime > 0)
                {
                    mre.WaitOne(remainingTime);
                }
                stopWatch.Reset();
                stopWatch.Start();
            }
        }
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<IEnumerable<YahooQuote>> GetHistoricPricesForTicker(string ticker)
    {
        string? urlToUse = configuration[Vendor];
        if (urlToUse == null)
        {
            logger.LogError("Could not find URL to obtain data from Yahoo....");
            return new List<YahooQuote>();
        }
        urlToUse = urlToUse.Replace("{ticker}", ticker)
            .Replace("{start}", start.ToString())
            .Replace("{end}", end.ToString());
        string values = string.Empty;
        try
        {
            values = await handleCache.GetStringAsnyc(urlToUse, CacheDuration.Days, 1);
        }
        catch (Exception ex)
        {
            logger.LogError("Error while getting values from Yahoo...");
            logger.LogError($"{ex.Message}");
            return new List<YahooQuote>();
        }
        if (string.IsNullOrEmpty(values))
        {
            logger.LogError("Could not obtain data from Yahoo....");
            logger.LogError($"URL used: {urlToUse}");
            return new List<YahooQuote>();
        }
        using StringReader quotesAsStream = new(values ?? "");
        using var csvReader = new CsvHelper.CsvReader(quotesAsStream, CultureInfo.InvariantCulture);
        _ = await csvReader.ReadAsync();
        _ = csvReader.ReadHeader();
        List<YahooQuote> quotes = new();
        try
        {
            while (await csvReader.ReadAsync())
            {
                quotes.Add(new YahooQuote
                {
                    Ticker = ticker,
                    Date = csvReader.GetField<DateTime>("Date"),
                    Open = csvReader.GetField<decimal>("Open"),
                    High = csvReader.GetField<decimal>("High"),
                    Low = csvReader.GetField<decimal>("Low"),
                    Close = csvReader.GetField<decimal>("Adj Close"),
                    Volume = csvReader.GetField<int>("Volume")
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to parse quotes obtained from Yahoo");
            logger.LogError($"{ex.Message}");
            return quotes;
        }
        return quotes;
    }

    private async Task<List<string>?> ObtainTickersToProcess()
    {
        try
        {
            var tickers = (await repository.FindAll())
                .Select(r => r.Ticker)
                .ToList();
            return tickers;
        }
        catch (Exception ex)
        {
            logger.LogError("Error reading values from database GetValuesFromYahoo:ObtainTickersToProcess");
            logger.LogError(ex.ToString());
            return null;
        }
    }

    #endregion Private Methods
}