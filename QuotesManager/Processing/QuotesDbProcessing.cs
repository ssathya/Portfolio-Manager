using ApplicationModels.Quotes;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace QuotesManager.Processing;

public class QuotesDbProcessing
{
    //private readonly IRepository<YQuotes> yRepository;
    private readonly IRepository<YPrice> ypRepository;

    private readonly ILogger<QuotesDbProcessing> logger;

    public QuotesDbProcessing(
          IRepository<YPrice> ypRepository
        , ILogger<QuotesDbProcessing> logger)
    {
        this.ypRepository = ypRepository;
        this.logger = logger;
    }

    public async Task<bool> ExecAsync(List<YPrice> yahooQuotes)
    {
        bool execResult = await RemoveOldEntiresAsync();
        if (!execResult)
        {
            return false;
        }
        execResult = await StoreQuotesInDb(yahooQuotes);
        return execResult;
    }

    private async Task<bool> StoreQuotesInDb(List<YPrice> yahooQuotes)
    {
        try
        {
            //List<YQuotes> quotes = mapper.Map<List<YQuotes>>(yahooQuotes);
            await ypRepository.Add(yahooQuotes);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to insert values to database QuotesDbProcessing:StoreQuotesInDb");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> RemoveOldEntiresAsync()
    {
        try
        {
            await ypRepository.Truncate();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to truncate table");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }
}