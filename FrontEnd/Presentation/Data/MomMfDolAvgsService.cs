using ApplicationModels.Compute;
using PsqlAccess;

namespace Presentation.Data;

public class MomMfDolAvgsService
{
    private readonly ILogger<MomMfDolAvgsService> logger;
    private readonly IRepository<MomMfDolAvg> repository;
    private static List<MomMfDolAvg> momMfDolAvgas = new();
    private static DateTimeOffset? createdTime;
    private static TimeSpan expiresAfter = TimeSpan.FromHours(5);

    public MomMfDolAvgsService(ILogger<MomMfDolAvgsService> logger
        , IRepository<MomMfDolAvg> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<List<MomMfDolAvg>> ExecAsync()
    {
        if (momMfDolAvgas.Any() && DateTimeOffset.UtcNow - createdTime <= expiresAfter)
        {
            return momMfDolAvgas;
        }
        try
        {
            var returnValues = await repository.FindAll();
            momMfDolAvgas = returnValues.ToList();
            createdTime = DateTimeOffset.UtcNow;
            return momMfDolAvgas;
        }
        catch (Exception ex)
        {
            logger.LogError("Error reading values from database");
            logger.LogError($"{ex.Message}");
            return new List<MomMfDolAvg>();
        }
    }
}