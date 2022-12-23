using ApplicationModels.Quotes;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace QuotesManager.Processing;

public class QuotesDbProcessing
{
    private readonly IRepository<YQuotes> yRepository;
    private readonly IMapper mapper;
    private readonly ILogger<QuotesDbProcessing> logger;

    public QuotesDbProcessing(IRepository<YQuotes> yRepository
        , IMapper mapper
        , ILogger<QuotesDbProcessing> logger)
    {
        this.yRepository = yRepository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<bool> ExecAsync(List<YahooQuote> yahooQuotes)
    {
        bool execResult = await RemoveOldEntiresAsync();
        if (!execResult)
        {
            return false;
        }
        execResult = await StoreQuotesInDb(yahooQuotes);
        return execResult;
    }

    private async Task<bool> StoreQuotesInDb(List<YahooQuote> yahooQuotes)
    {
        try
        {
            List<YQuotes> quotes = mapper.Map<List<YQuotes>>(yahooQuotes);
            await yRepository.Add(quotes);
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
            await yRepository.Truncate();
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