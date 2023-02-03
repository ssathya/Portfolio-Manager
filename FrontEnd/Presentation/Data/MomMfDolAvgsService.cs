using ApplicationModels.Compute;
using PsqlAccess;

namespace Presentation.Data;

public class MomMfDolAvgsService
{
    private readonly ILogger<MomMfDolAvgsService> logger;
    private readonly IRepository<MomMfDolAvg> repository;

    public MomMfDolAvgsService(ILogger<MomMfDolAvgsService> logger
        , IRepository<MomMfDolAvg> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<List<MomMfDolAvg>> ExecAsync()
    {
        try
        {
            var returnValues = await repository.FindAll();
            return returnValues.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError("Error reading values from database");
            logger.LogError($"{ex.Message}");
            return new List<MomMfDolAvg>();
        }
    }
}