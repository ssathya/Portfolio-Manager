using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarnCal.Processing;

public class ConsumeYahooEc
{
    private readonly ILogger<ConsumeYahooEc> logger;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly ReadYahooValues readYahooValues;
    private const int MaxNumberOfCalls = 20;
    private readonly DateTime defaultDt = new DateTime(1900, 1, 1).ToUniversalTime();

    public ConsumeYahooEc(ILogger<ConsumeYahooEc> logger
        , IRepository<IndexComponent> idxRepository
        , IRepository<ApplicationModels.EarningsCal.EarningsCalendar> ecRepository
        , ReadYahooValues readYahooValues)
    {
        this.logger = logger;
        this.idxRepository = idxRepository;
        this.ecRepository = ecRepository;
        this.readYahooValues = readYahooValues;
    }

    public async Task<List<YahooEarningCal>> GetValuesFromYahoo()
    {
        List<string> tickersProcessed = (await ecRepository.FindAll())
            .Where(x => x.DataObtained == true)
            .Select(x => x.Ticker).ToList();
        Random rng = new();
        List<string> tickersToProcess = (await idxRepository
                .FindAll(x => !tickersProcessed.Contains(x.Ticker)))
            .Select(x => x.Ticker)
            .OrderBy(a => rng.Next())
            .Take(MaxNumberOfCalls)
            .ToList();
        List<YahooEarningCal> earningsDates = new();
        foreach (var ticker in tickersToProcess)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                continue;
            }
            DateTime earningsDate = await readYahooValues.GetValuesFromWeb(ticker);
            if (earningsDate.Equals(defaultDt))
            {
                continue;
            }
            earningsDates.Add(new YahooEarningCal
            {
                Symbol = ticker,
                ReportingDate = earningsDate
            });
        }
        return earningsDates;
    }
}