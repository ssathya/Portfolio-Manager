using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarningsCalendar.Processing;

public class EarningsCalToDb
{
    private readonly IRepository<ApplicationModels.EarningsCal.EarningsCalendar> ecRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<EarningsCalToDb> logger;

    public EarningsCalToDb(IRepository<IndexComponent> idxRepository
        , IRepository<ApplicationModels.EarningsCal.EarningsCalendar> ecRepository
        , ILogger<EarningsCalToDb> logger)
    {
        this.idxRepository = idxRepository;
        this.ecRepository = ecRepository;
        this.logger = logger;
    }

    public async Task<bool> UpdateFinnHubData(FinnhubCal finnhubCal)
    {
        if (finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            return false;
        }
        List<string> tickersToProcess;
        List<string> indexIndustries;
        IEnumerable<ApplicationModels.EarningsCal.EarningsCalendar>? earingsCalInDb;
        try
        {
            (tickersToProcess, indexIndustries, earingsCalInDb)
                = await GetTickersFromDatabase(finnhubCal);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read data from database EarningsCalToDb:UpdateFinnHubData");
            logger.LogError(ex.ToString());
            return false;
        }
        if (!indexIndustries.Any())
        {
            return true;
        }
        Earningscalendar[] earningscalendars = finnhubCal.EarningsCalendar;
        try
        {
            if (earingsCalInDb.Any())
            {
                await UpdateExistingRecords(earingsCalInDb, earningscalendars);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update data to database EarningsCalToDb:UpdateFinnHubData");
            logger.LogError(ex.ToString());
            return false;
        }

        try
        {
            IEnumerable<Earningscalendar> earnCal = finnhubCal.EarningsCalendar.Where(x => indexIndustries.Contains(x.Symbol));
            await AddNewRecordsToDb(earnCal, earingsCalInDb);
            await RemoveAgedRecords();
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to Add/Remove values to/from database EarningsCalToDb:UpdateFinnHubData");
            logger.LogError(ex.ToString());
            return false;
        }
        return true;
    }

    private async Task AddNewRecordsToDb(IEnumerable<Earningscalendar> earnCal, IEnumerable<ApplicationModels.EarningsCal.EarningsCalendar> earingsCalInDb)
    {
        IEnumerable<string> tickersInDb = earingsCalInDb.Select(x => x.Ticker);
        IEnumerable<Earningscalendar> earningscalendars = earnCal.Where(x => !tickersInDb.Contains(x.Symbol));
        List<ApplicationModels.EarningsCal.EarningsCalendar> newEarningCals = new();
        var defaultDate = new DateTime(1900, 1, 1).ToUniversalTime();
        foreach (var ec in earningscalendars)
        {
            newEarningCals.Add(new ApplicationModels.EarningsCal.EarningsCalendar()
            {
                Ticker = ec.Symbol,
                VendorEarningsDate = ec.Date.ToUniversalTime(),
                RemoveDate = DateTime.UtcNow.AddMonths(1),
                EarningsDateYahoo = defaultDate,
                EarningsReadDate = defaultDate
            });
        }
        if (newEarningCals.Any())
        {
            await ecRepository.Add(newEarningCals);
        }
    }

    private async Task<(List<string>, List<string>, IEnumerable<ApplicationModels.EarningsCal.EarningsCalendar>)>
        GetTickersFromDatabase(FinnhubCal finnhubCal)
    {
        List<string> tickersToProcess;
        if (finnhubCal.EarningsCalendar != null && finnhubCal.EarningsCalendar.Length != 0)
        {
            tickersToProcess = finnhubCal.EarningsCalendar.Select(x => x.Symbol).ToList();
        }
        else
        {
            tickersToProcess = new List<string>();
        }
        List<string> indexIndustries = (await idxRepository.FindAll(x => tickersToProcess.Contains(x.Ticker)))
            .Select(x => x.Ticker)
            .ToList();
        List<ApplicationModels.EarningsCal.EarningsCalendar> earingsCalInDb = new();
        try
        {
            earingsCalInDb = (await ecRepository.FindAll(x => tickersToProcess.Contains(x.Ticker)))
                .Where(x => x.DataObtained == false)
               .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError("Error obtaining data from Earnings Calendar repository");
            logger.LogError($"{ex.Message}");
        }
        return (tickersToProcess, indexIndustries, earingsCalInDb);
    }

    private async Task RemoveAgedRecords()
    {
        var earingsCalToRemove = await ecRepository.FindAll(x => x.RemoveDate <= DateTime.UtcNow);
        if (earingsCalToRemove.Any())
        {
            await ecRepository.Remove(earingsCalToRemove);
        }
    }

    private async Task UpdateExistingRecords(IEnumerable<ApplicationModels.EarningsCal.EarningsCalendar> earingsCalInDb, Earningscalendar[] earningscalendars1)
    {
        foreach (var ecInDb in earingsCalInDb)
        {
            var updatedVendorData = earningscalendars1.FirstOrDefault(x => x.Symbol.Equals(ecInDb.Ticker, StringComparison.OrdinalIgnoreCase));
            if (updatedVendorData != null)
            {
                ecInDb.VendorEarningsDate = updatedVendorData.Date.ToUniversalTime();
                ecInDb.RemoveDate = DateTime.UtcNow.AddMonths(1);
            }
        }
        await ecRepository.Update(earingsCalInDb);
    }
}