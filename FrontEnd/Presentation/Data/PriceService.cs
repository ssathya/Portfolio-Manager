using ApplicationModels.Quotes;
using PsqlAccess;

namespace Presentation.Data;

public class PriceService
{
    private readonly ILogger<PriceService> logger;
    private readonly IRepository<YPrice> priceRepository;

    public PriceService(ILogger<PriceService> logger
        , IRepository<YPrice> priceRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
    }

    public async Task<YPrice?> ExecAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return null;
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
}