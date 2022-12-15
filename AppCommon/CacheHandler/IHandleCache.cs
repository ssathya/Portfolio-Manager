namespace AppCommon.CacheHandler;

public interface IHandleCache
{
    Task<T?> GetAsync<T>(string url,
        CacheDuration cacheDuration = CacheDuration.Minutes,
        int minutes = 60,
        bool forceRefresh = false,
        string nullReplace = "");

    Task<string> GetStringAsnyc(string url,
        CacheDuration cacheDuration = CacheDuration.Minutes,
        int minutes = 60,
        bool forceRefresh = false);
}

public enum CacheDuration
{
    Seconds,
    Minutes,
    Hours,
    Days
}