using AppCommon.CacheHandler;
using ApplicationModels.Compute;
using ApplicationModels.Indexes;
using ApplicationModels.SimFin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace ManageSimFinRatings.Processing;

public class ObtainSimFinRatings
{
    private readonly IConfiguration configuration;
    private readonly IHandleCache handleCache;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<ObtainSimFinRatings> logger;
    private readonly IRepository<ScoreDetail> scoreDetailRepository;
    private readonly IRepository<SimFinRatio> simFinRatioRepository;
    private List<ScoreDetail> scoreDetails = new();
    private List<SimFinRatio> simFinRatios = new();
    private List<string> Tickers = new();
    private readonly Random random = new Random();

    public ObtainSimFinRatings(ILogger<ObtainSimFinRatings> logger
        , IRepository<ScoreDetail> scoreDetailRepository
        , IRepository<IndexComponent> idxRepository
        , IRepository<SimFinRatio> simFinRatioRepository
        , IHandleCache handleCache
        , IConfiguration configuration)
    {
        this.logger = logger;
        this.scoreDetailRepository = scoreDetailRepository;
        this.idxRepository = idxRepository;
        this.simFinRatioRepository = simFinRatioRepository;
        this.handleCache = handleCache;
        this.configuration = configuration;
    }

    public async Task<bool> ExecAsync()
    {
        await PopulateTickers();
        if (Tickers.Count == 0)
        {
            logger.LogError("Could not get Index Components");
            return false;
        }
        await PopulateScoreDetails();
        int counter = 0;
        foreach (var ticker in Tickers)
        {
            await ObtainRatiosAsync(ticker);
            if (++counter % 20 == 0)
            {
                logger.LogInformation($"Last ticker processed was {ticker}");
                logger.LogInformation($"Processed {counter} out of {Tickers.Count}");
            }
        }
        bool updateValues = await UpdateScoreDetails();
        if (!updateValues)
        {
            return false;
        }
        updateValues = await UpdateSimFinRatios();
        return updateValues;
    }

    private async Task ObtainRatiosAsync(string ticker)
    {
        var urlToUse = configuration["Ratios"];
        if (string.IsNullOrEmpty(urlToUse))
        {
            logger.LogCritical("Configuration error");
            return;
        }
        if (!GetEntries.EntryDictionary.ContainsKey(ticker))
        {
            logger.LogInformation($"Vendor has no information about {ticker}");
            return;
        }
        urlToUse = urlToUse.Replace("{firm}", GetEntries.EntryDictionary[ticker].ToString());
        List<Indicator>? allRatios = await handleCache.GetAsync<List<Indicator>>(urlToUse, CacheDuration.Days, random.Next(20, 30));
        if (allRatios == null || allRatios.Count == 0)
        {
            logger.LogError($"Vendor did not provide values for {ticker}");
            return;
        }
        simFinRatios.Add(new()
        {
            Ticker = ticker,
            ProcessingDate = DateTime.Now.Date.ToUniversalTime(),
            Indicators = allRatios
        });
        Indicator? indicator = allRatios.FirstOrDefault(pfs => pfs.IndicatorName == "Piotroski F-Score");
        if (indicator != null)
        {
            var scoreDetail = scoreDetails.FirstOrDefault(x => x.Ticker.Equals(ticker));
            if (scoreDetail != null)
            {
                scoreDetail.SimFinRating = (int)indicator.Value;
            }
            else
            {
                scoreDetails.Add(new()
                {
                    Ticker = ticker,
                    LastEarningsDate = (indicator.PeriodEndDate ?? DateTime.Now.Date).ToUniversalTime(),
                    SimFinRating = (int)indicator.Value,
                });
            }
        }
    }

    private async Task PopulateScoreDetails()
    {
        scoreDetails = (await scoreDetailRepository.FindAll())
            .ToList();
    }

    private async Task PopulateTickers()
    {
        var Indexs = (await idxRepository.FindAll())
            .ToList();
        if (!Indexs.Any())
        {
            return;
        }
        Tickers = Indexs.Select(i => i.Ticker).ToList();
    }

    private async Task<bool> UpdateScoreDetails()
    {
        try
        {
            var scores = (await scoreDetailRepository.FindAll())
                .ToList();
            foreach (var score in scores)
            {
                var updatedScore = scoreDetails.FirstOrDefault(x => x.Ticker.Equals(score.Ticker));
                if (updatedScore != null)
                {
                    score.SimFinRating = updatedScore.SimFinRating;
                    scoreDetails.Remove(updatedScore);
                }
            }
            await scoreDetailRepository.Update(scores);
            await scoreDetailRepository.Add(scoreDetails);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update Score Details");
            logger.LogError(ex.Message);
            return false;
        }
        return true;
    }

    private async Task<bool> UpdateSimFinRatios()
    {
        try
        {
            await simFinRatioRepository.Truncate();
            await simFinRatioRepository.Add(simFinRatios);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update SimFinRatios");
            logger.LogError(ex.Message);
            return false;
        }
        return true;
    }
}