using ApplicationModels.Views;
using PsqlAccess;

namespace Presentation.Data;

public class SecurityWithPScoresService
{
    private readonly ILogger<SecurityWithPScoresService> logger;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;
    private static List<SecurityWithPScore>? cachedValues = null;
    private static DateTimeOffset? createdTime;
    private static TimeSpan expiresAfter = TimeSpan.FromHours(5);

    public SecurityWithPScoresService(ILogger<SecurityWithPScoresService> logger
        , IRepository<SecurityWithPScore> secScoreRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
    }

    public async Task<List<SecurityWithPScore>> ExecAsync()
    {
        if (cachedValues != null && DateTimeOffset.UtcNow - createdTime <= expiresAfter)
        {
            return cachedValues;
        }
        try
        {
            IEnumerable<SecurityWithPScore> returnValue = await secScoreRepository.FindAll();
            foreach (var swps in returnValue)
            {
                FixNullValues(swps);
            }
            cachedValues = returnValue.ToList();
            createdTime = DateTimeOffset.UtcNow;
            return cachedValues;
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