using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarnCal.Processing;

public class EarningsCalToDb
{
    #region Private Fields

    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<EarningsCalToDb> logger;

    #endregion Private Fields

    #region Public Constructors

    public EarningsCalToDb(IRepository<IndexComponent> idxRepository
        , IRepository<EarningsCalendar> ecRepository
        , ILogger<EarningsCalToDb> logger)
    {
        this.idxRepository = idxRepository;
        this.ecRepository = ecRepository;
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> UpdateFinnHubData(FinnhubCal finnhubCal)
    {
        if (finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            return false;
        }
        List<string> tickersToProcess;
        List<string> indexIndustries;
        IEnumerable<EarningsCalendar>? earingsCalInDb;
        try
        {
            (tickersToProcess, indexIndustries, earingsCalInDb)
                = await GetTickersFromDatabase(finnhubCal);
            await RemoveProccessedRecords(finnhubCal);
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

    public async Task<bool> UpdateYahooEarningsCal(List<YahooEarningCal> yahooEarningsDates)
    {
        List<string> tickersToProcess = yahooEarningsDates.Select(x => x.Symbol).ToList();
        IEnumerable<EarningsCalendar>? existingRecords = await UpdateExistingRecords(yahooEarningsDates, tickersToProcess);
        if (existingRecords == null)
        {
            return false;
        }

        bool updateResult = await AddNewRecords(yahooEarningsDates, tickersToProcess, existingRecords);
        return updateResult;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<bool> AddNewRecords(List<YahooEarningCal> yahooEarningsDates, List<string> tickersToProcess, IEnumerable<EarningsCalendar> existingRecords)
    {
        List<string> existingTickers = existingRecords.Select(x => x.Ticker).ToList();
        tickersToProcess = tickersToProcess.Except(existingTickers).ToList();
        var defaultDate = new DateTime(2100, 1, 1).ToUniversalTime();
        List<EarningsCalendar> newEarningCals = new();
        foreach (var tickerToProcess in tickersToProcess)
        {
            var yahooDate = yahooEarningsDates.FirstOrDefault(x => x.Symbol.Equals(tickerToProcess, StringComparison.OrdinalIgnoreCase));
            if (yahooDate == default)
            {
                continue;
            }
            newEarningCals.Add(new EarningsCalendar()
            {
                Ticker = tickerToProcess,
                VendorEarningsDate = defaultDate,
                RemoveDate = DateTime.UtcNow.AddMonths(1),
                EarningsDateYahoo = yahooDate.ReportingDate.ToUniversalTime(),
                EarningsReadDate = defaultDate
            });
        }
        if (newEarningCals.Any())
        {
            try
            {
                await ecRepository.Add(newEarningCals);
            }
            catch (Exception ex)
            {
                logger.LogError("Unable to add dataset EarningsCalendar in method EarningsCalToDb:UpdateExistingRecords");
                logger.LogError(ex.ToString());
                return false;
            }
        }
        return true;
    }

    private async Task AddNewRecordsToDb(IEnumerable<Earningscalendar> earnCal, IEnumerable<EarningsCalendar> earingsCalInDb)
    {
        IEnumerable<string> tickersInDb = earingsCalInDb.Select(x => x.Ticker);
        IEnumerable<Earningscalendar> earningscalendars = earnCal.Where(x => !tickersInDb.Contains(x.Symbol));
        List<EarningsCalendar> newEarningCals = new();
        var defaultDate = new DateTime(2100, 1, 1).ToUniversalTime();
        foreach (var ec in earningscalendars)
        {
            newEarningCals.Add(new EarningsCalendar()
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

    private async Task<(List<string>, List<string>, IEnumerable<EarningsCalendar>)>
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
        List<EarningsCalendar> earingsCalInDb = new();
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

    private async Task RemoveProccessedRecords(FinnhubCal finnhubCal)
    {
        var processedEntries = (await ecRepository.FindAll(x => x.DataObtained == true))
            .ToList();
        if (finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            return;
        }
        var removeRecords = finnhubCal.EarningsCalendar.Where(x => processedEntries.Select(x => x.Ticker).ToList().Contains(x.Symbol))
            .Select(x => x.Symbol);
        if (removeRecords == null || removeRecords.Count() == 0)
        {
            return;
        }
        var earningsCalendar = finnhubCal.EarningsCalendar.ToList();
        earningsCalendar.RemoveAll(x => removeRecords.Contains(x.Symbol));
        finnhubCal.EarningsCalendar = earningsCalendar.ToArray();
    }

    private async Task UpdateExistingRecords(IEnumerable<EarningsCalendar> earingsCalInDb, Earningscalendar[] earningscalendars1)
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

    private async Task<IEnumerable<EarningsCalendar>?> UpdateExistingRecords(List<YahooEarningCal> yahooEarningsDates, List<string> tickersToProcess)
    {
        try
        {
            IEnumerable<EarningsCalendar> existingRecords = await ecRepository.FindAll(x => tickersToProcess.Contains(x.Ticker));
            foreach (var ec in existingRecords)
            {
                var yahooDate = yahooEarningsDates.FirstOrDefault(x => x.Symbol.Equals(ec.Ticker, StringComparison.OrdinalIgnoreCase));
                if (yahooDate == null)
                {
                    continue;
                }
                ec.EarningsDateYahoo = yahooDate.ReportingDate;
            }
            await ecRepository.Update(existingRecords);
            return existingRecords;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update dataset EarningsCalendar in method EarningsCalToDb:UpdateExistingRecords");
            logger.LogError(ex.ToString());
        }
        return null;
    }

    #endregion Private Methods
}