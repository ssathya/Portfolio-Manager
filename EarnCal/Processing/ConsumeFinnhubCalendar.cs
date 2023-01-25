using AppCommon.CacheHandler;
using ApplicationModels.EarningsCal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EarnCal.Processing;

public class ConsumeFinnhubCalendar
{
    private readonly IConfiguration configuration;
    private readonly ILogger<ConsumeFinnhubCalendar> logger;
    private readonly IHandleCache handleCache;
    private const string Vendor = "finnhub";
    private const string ISODateFormat = "yyyy-MM-dd";
    private const int startDateOffset = -20;
    private const int endDateOffset = -2;

    public ConsumeFinnhubCalendar(IConfiguration configuration
        , ILogger<ConsumeFinnhubCalendar> logger
        , IHandleCache handleCache)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.handleCache = handleCache;
    }

    public async Task<FinnhubCal?> GetValuesFromVendor()
    {
        string? urlToUse = configuration[Vendor];
        if (urlToUse == null)
        {
            logger.LogError("Could not find URL to obtain data from Finnhub....");
            return null;
        }
        var startDate = DateTimeOffset.UtcNow.AddDays(startDateOffset).ToString(ISODateFormat);
        var endDate = DateTimeOffset.UtcNow.AddDays(endDateOffset).ToString(ISODateFormat);
        urlToUse = urlToUse.Replace(@"{startDate}", startDate)
           .Replace(@"{endDate}", endDate);
        FinnhubCal? finnhubCal = await handleCache.GetAsync<FinnhubCal>(urlToUse, CacheDuration.Hours, 23);
        if (finnhubCal == null || finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            logger.LogError("Vendor data was inconstant with contract");
        }

        return finnhubCal;
    }
}