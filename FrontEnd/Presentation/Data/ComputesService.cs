using ApplicationModels.Compute;
using PsqlAccess;

namespace Presentation.Data;

public class ComputesService
{
    private readonly ILogger<ComputesService> logger;
    private readonly IRepository<Compute> repository;

    public ComputesService(ILogger<ComputesService> logger
        , IRepository<Compute> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<Compute?> ExecAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return null;
        }
        try
        {
            Compute? compute = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (compute == null)
            {
                logger.LogInformation($"Could not get computed values for {ticker}");
                return null;
            }
            return compute;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting compute values for {ticker}");
            logger.LogError(ex.ToString());
            return null;
        }
    }
}