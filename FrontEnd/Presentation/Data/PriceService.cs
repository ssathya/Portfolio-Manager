using ApplicationModels.Compute;
using ApplicationModels.Quotes;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using PsqlAccess;

namespace Presentation.Data;

public class PriceService
{
    private readonly ILogger<PriceService> logger;
    private readonly IRepository<YPrice> priceRepository;
    private readonly IRepository<Compute> computeRepository;
    private static List<YPrice>? priceCache = null;
    private static DateTimeOffset? createdTime;
    private static TimeSpan expiresAfter = TimeSpan.FromHours(5);

    public PriceService(ILogger<PriceService> logger
        , IRepository<YPrice> priceRepository
    , IRepository<Compute> computeRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
        this.computeRepository = computeRepository;
    }

    public async Task<List<YPrice>?> ExecAsync()
    {
        if (priceCache != null && DateTimeOffset.UtcNow - createdTime <= expiresAfter)
        {
            return priceCache;
        }
        try
        {
            IEnumerable<YPrice> priceForAllSecurities = await priceRepository.FindAll();
            priceCache = priceForAllSecurities.ToList();
            createdTime = DateTimeOffset.UtcNow;
            return priceCache;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading pricing details");
            logger.LogError(ex.ToString());
            return null;
        }
    }

    public async Task<YPrice?> ExecAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return null;
        }

        YPrice? prices = await ExtractValueFromDb(ticker);
        return prices;
    }

    private async Task<YPrice?> ExtractValueFromDb(string ticker)
    {
        if (priceCache != null && DateTimeOffset.Now - createdTime <= expiresAfter)
        {
            var priceForSecurity = priceCache.Find(x => x.Ticker == ticker.ToUpper().Trim());
            if (priceForSecurity != null)
            {
                return priceForSecurity;
            }
        }
        try
        {
            YPrice? priceForSecurity = (await priceRepository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (priceForSecurity == null)
            {
                logger.LogInformation($"Price for security {ticker} not in database");
                return null;
            }
            return priceForSecurity;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading pricing for security {ticker}");
            logger.LogError(ex.ToString());
            return null;
        }
    }

    public async Task<Compute?> GetComputedValues(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return new Compute();
        }
        try
        {
            Compute? computedValues = (await computeRepository.FindAll(r => r.Ticker.Equals(ticker.ToUpper().Trim())))
               .FirstOrDefault();
            return computedValues;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading computed values for security {ticker}");
            logger.LogError(ex.Message.ToString());
            return null;
        }
    }
}