using AppCommon.CacheHandler;
using ApplicationModels.SimFin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ManageSimFinRatings.Processing;

public class GetEntries
{
    private const string ApplicationName = "SimFinHandler";
    internal static Dictionary<string, int> EntryDictionary = new();
    private readonly ILogger<GetEntries> logger;
    private readonly IHandleCache handleCache;
    private readonly IConfiguration configuration;

    public GetEntries(ILogger<GetEntries> logger
        , IHandleCache handleCache
        , IConfiguration configuration)
    {
        this.logger = logger;
        this.handleCache = handleCache;
        this.configuration = configuration;
    }

    public async Task<bool> ExcecAsync()
    {
        bool ExtractResult = await ObtainConversionValuesAsync();
        return ExtractResult;
    }

    private async Task<bool> ObtainConversionValuesAsync()
    {
        var urlToUse = configuration["AllEntitites"];
        if (string.IsNullOrEmpty(urlToUse))
        {
            logger.LogCritical("Configuration Error");
            return false;
        }
        var responseValues = await handleCache.GetAsync<List<Entry>>(urlToUse, CacheDuration.Days, 15, false);
        if (responseValues == null || responseValues.Count == 0)
        {
            logger.LogError("No dictironary values from SimFin");
            return false;
        }
        EntryDictionary.Clear();
        EntryDictionary = (from a in responseValues
                           where !string.IsNullOrEmpty(a.Ticker)
                           select (a.Ticker, a.SimId))
                           .ToDictionary(b => b.Ticker, b => b.SimId);
        return true;
    }
}