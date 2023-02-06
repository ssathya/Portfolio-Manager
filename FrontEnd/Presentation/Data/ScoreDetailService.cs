using ApplicationModels.Compute;
using PsqlAccess;

namespace Presentation.Data;

public class ScoreDetailService
{
    private readonly ILogger<ScoreDetailService> logger;
    private readonly IRepository<ScoreDetail> repository;

    public ScoreDetailService(ILogger<ScoreDetailService> logger
        , IRepository<ScoreDetail> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<ScoreDetail> ExecAsync(string ticker)
    {
        try
        {
            ScoreDetail? scoreDetail = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (scoreDetail == null)
            {
                return new ScoreDetail { Ticker = ticker };
            }
            return scoreDetail;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading Score Detail for ticker {ticker}");
            logger.LogError(ex.ToString());
            return new ScoreDetail { Ticker = ticker };
        }
    }
}