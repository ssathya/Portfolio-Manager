using ApplicationModels.FinancialStatement.AlphaVantage;
using PsqlAccess;

namespace Presentation.Data;

public class OverviewService
{
    private readonly ILogger<OverviewService> logger;
    private readonly IRepository<Overview> repository;

    public OverviewService(ILogger<OverviewService> logger
        , IRepository<Overview> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<Overview?> ExecAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return null;
        }
        try
        {
            Overview? overview = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (overview == null)
            {
                logger.LogInformation($"Overview for security {ticker} not in database");
                return null;
            }
            return overview;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting Overview for security {ticker} from database");
            logger.LogError(ex.ToString());
            return null;
        }
    }
}