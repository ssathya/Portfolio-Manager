using Microsoft.Extensions.Logging;
using MonkeyCache;
using MonkeyCache.SQLite;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AppCommon.CacheHandler;

public class HandleCache : IHandleCache
{
    private static object? lockingObj;
    private readonly IBarrel currentBarrel;
    private readonly HttpClient httpClient;
    private readonly ILogger<HandleCache> logger;

    public HandleCache(HttpClient httpClient, ILogger<HandleCache> logger)
    {
        this.httpClient = httpClient;
        //Even 5 seconds is too long.
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        this.logger = logger;
        currentBarrel = Barrel.Current;
    }

    /// <summary>
    /// Gets the string asnyc.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="cacheDuration">Duration of the cache enumerator Default minutes.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
    /// <returns></returns>
    public async Task<string> GetStringAsnyc(string url, CacheDuration cacheDuration = CacheDuration.Minutes, int duration = 60, bool forceRefresh = false)
    {
        var json = string.Empty;
        try
        {
            lockingObj ??= new object();
            lock (lockingObj)
            {
                currentBarrel.EmptyExpired();
            }
            if (!forceRefresh && !currentBarrel.IsExpired(url))
            {
                json = currentBarrel.Get<string>(url);
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await httpClient.GetStringAsync(url);
                lock (lockingObj)
                {
                    var expirln = cacheDuration switch
                    {
                        CacheDuration.Seconds => TimeSpan.FromSeconds(duration),
                        CacheDuration.Minutes => TimeSpan.FromSeconds(duration),
                        CacheDuration.Hours => TimeSpan.FromHours(duration),
                        CacheDuration.Days => TimeSpan.FromDays(duration),
                        _ => TimeSpan.FromMinutes(duration),
                    };
                    currentBarrel.Add(url, json, expirln);
                }
            }
            json = json.Replace(":\"N/A\"", ":null");

            return json;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error while processing data from {url}");
            logger.LogError(ex.Message);
        }
        return "";
    }

    /// <summary>
    /// Gets the asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">The URL.</param>
    /// <param name="cacheDuration">Duration of the cache.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
    /// <returns></returns>
    public async Task<T?> GetAsync<T>(string url, CacheDuration cacheDuration = CacheDuration.Minutes, int duration = 60, bool forceRefresh = false, string nullReplace = "")
    {
        string json = await GetStringAsnyc(url, cacheDuration, duration, forceRefresh);
        //Handle a special case where dates are marked as None.
        string pattern = @"Date\"": \""None\""";
        string substitution = "Date\": \"1900-01-01\"";
        RegexOptions options = RegexOptions.Multiline;
        Regex regex = new Regex(pattern, options);
        json = regex.Replace(json, substitution);

        if (!string.IsNullOrWhiteSpace(nullReplace))
        {
            json = Regex.Replace(json, nullReplace, "\"0\"");
        }
        if (!string.IsNullOrWhiteSpace(json))
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings());
        }
        return default;
    }

    private static JsonSerializerSettings SerializerSettings()
    {
        return new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
    }
}