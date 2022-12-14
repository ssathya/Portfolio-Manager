using MonkeyCache.SQLite;
using System.Runtime.InteropServices;

namespace AppCommon.CacheHandler;

public static class CacheInitialize
{
    #region Public Methods

    public static void Initialize(string appId)
    {
        var cachePath = """~/barrel/""";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            Barrel.Create(cachePath);
        }
        else
        {
            cachePath = Path.GetTempPath();
            Barrel.Create(cachePath);
        }
        Barrel.ApplicationId = appId;
    }

    #endregion Public Methods
}