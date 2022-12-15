using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
using CosminSanda.Finance.Records;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using CosminSanda.Finance;

namespace EarnCal.Processing;

public class ConsumeYahooEc
{
    private readonly ILogger<ConsumeYahooEc> logger;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly IRepository<ApplicationModels.EarningsCal.EarningsCalendar> ecRepository;
    private const int MaxNumberOfCalls = 20;

    public ConsumeYahooEc(ILogger<ConsumeYahooEc> logger
        , IRepository<IndexComponent> idxRepository
        , IRepository<ApplicationModels.EarningsCal.EarningsCalendar> ecRepository)
    {
        this.logger = logger;
        this.idxRepository = idxRepository;
        this.ecRepository = ecRepository;
    }

    public async Task<List<YahooEarningCal>> GetValuesFromYahoo()
    {
        List<string> tickersProcessed = (await ecRepository.FindAll())
            .Where(x => x.DataObtained == true)
            .Select(x => x.Ticker).ToList();
        Random rng = new Random();
        List<string> tickersToProcess = (await idxRepository
                .FindAll(x => !tickersProcessed.Contains(x.Ticker)))
            .Select(x => x.Ticker)
            .OrderBy(a => rng.Next())
            .Take(MaxNumberOfCalls)
            .ToList();
        List<YahooEarningCal> earningsDates = new();
        foreach (var ticker in tickersToProcess)
        {
            List<EarningsDate> earnings = await CosminSanda.Finance.EarningsCalendar.GetPastEarningsDates(ticker);
            if (earnings != null && earnings.Count > 0)
            {
                var lastEarningDt = earnings.Last();
                earningsDates.Add(new YahooEarningCal
                {
                    Symbol = ticker,
                    ReportingDate = new DateTime(lastEarningDt.Date.Year
                    , lastEarningDt.Date.Month
                    , lastEarningDt.Date.Day)
                    .ToUniversalTime()
                });
            }
        }
        return earningsDates;
    }
}