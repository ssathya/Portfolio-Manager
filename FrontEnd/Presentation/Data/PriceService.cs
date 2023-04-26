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

    public PriceService(ILogger<PriceService> logger
        , IRepository<YPrice> priceRepository
    , IRepository<Compute> computeRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
        this.computeRepository = computeRepository;
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