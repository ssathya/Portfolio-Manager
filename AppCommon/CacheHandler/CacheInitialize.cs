using MonkeyCache.SQLite;
using System.Runtime.InteropServices;

namespace AppCommon.CacheHandler;

public static class CacheInitialize
{
    #region Public Methods

    public static void Initialize(string appId)
    {
        Barrel.ApplicationId = appId;
        var cachePath = Environment.GetEnvironmentVariable("HOME");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            cachePath += $"/cache/{appId}";
            DirectoryInfo di;
            if (!Directory.Exists(cachePath))
            {
                di = Directory.CreateDirectory(cachePath);
            }
            else
            {
                di = new DirectoryInfo(cachePath);
            }
            if (di.Exists)
            {
                Barrel.Create(cachePath);
            }
        }
        else
        {
            cachePath = Path.GetTempPath();
            Barrel.Create(cachePath);
        }
    }

    #endregion Public Methods
}