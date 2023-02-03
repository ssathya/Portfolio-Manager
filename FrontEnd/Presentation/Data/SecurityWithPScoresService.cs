using ApplicationModels.Views;
using PsqlAccess;

namespace Presentation.Data;

public class SecurityWithPScoresService
{
    private readonly ILogger<SecurityWithPScoresService> logger;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;

    public SecurityWithPScoresService(ILogger<SecurityWithPScoresService> logger
        , IRepository<SecurityWithPScore> secScoreRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
    }

    public async Task<List<SecurityWithPScore>> ExecAsync()
    {
        try
        {
            IEnumerable<SecurityWithPScore> returnValue = await secScoreRepository.FindAll();
            foreach (var swps in returnValue)
            {
                FixNullValues(swps);
            }
            return returnValue.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError("Did not find any records in database");
            logger.LogError(ex.Message);
        }
        return new List<SecurityWithPScore>();
    }

    private static void FixNullValues(SecurityWithPScore swps)
    {
        swps.PiotroskiComputedValue = swps.PiotroskiComputedValue ?? 0;
        swps.SimFinRating = swps.SimFinRating ?? 0;
    }
}