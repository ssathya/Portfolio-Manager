using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace EarnCal.Processing;

public class ExceptionReporting
{
    #region Private Fields

    private const int daysToHoldReport = 7;
    private const int maxDaysToWait = 7;
    private const int YahooVsFinnHubDays = 5;
    private readonly IRepository<EarningsCalendar> ecRepository;
    private readonly IRepository<EarningsCalExceptions> exceptionRepository;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<ExceptionReporting> logger;

    #endregion Private Fields

    #region Public Constructors

    public ExceptionReporting(IRepository<EarningsCalExceptions> exceptionRepository
        , IRepository<IndexComponent> idxRepository
        , IRepository<EarningsCalendar> ecRepository
        , ILogger<ExceptionReporting> logger)
    {
        this.exceptionRepository = exceptionRepository;
        this.idxRepository = idxRepository;
        this.ecRepository = ecRepository;
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> BuildReport()
    {
        bool operationResult = await DropAgedRecords();
        if (operationResult == false)
        {
            logger.LogError("DropAgedRecords failed");
            return false;
        }
        operationResult = await GenerateTickerNotInIndex();
        if (operationResult == false)
        {
            logger.LogError("GenerateTickerNotInIndex failed");
            return false;
        }
        operationResult = await GenerateDateDiffReport();
        if (operationResult == false)
        {
            logger.LogError("GenerateDateDiffReport failed");
            return false;
        }
        operationResult = await DataNotProcessedTooLong();
        if (operationResult == false)
        {
            logger.LogError("DataNotProcessedTooLong failed");
            return false;
        }
        operationResult = await StaleRecordHeldInDatabase();
        if (operationResult == false)
        {
            logger.LogError("StaleRecordHeldInDatabase failed");
            return false;
        }
        return operationResult;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<bool> DataNotProcessedTooLong()
    {
        IEnumerable<EarningsCalendar> ecContent;
        try
        {
            ecContent = await ecRepository.FindAll(x => x.DataObtained == false);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read data from database ExceptionReporting:DataNotProcessedTooLong");
            logger.LogError(ex.ToString());
            return false;
        }
        List<EarningsCalExceptions> earningsCalExceptions = new();
        foreach (var ec in ecContent)
        {
            var yahooDateDiff = (DateTime.UtcNow - ec.EarningsDateYahoo).Days;
            var finnHubDateDiff = (DateTime.UtcNow - ec.VendorEarningsDate).Days;
            if (yahooDateDiff > maxDaysToWait || finnHubDateDiff > maxDaysToWait)
            {
                earningsCalExceptions.Add(new()
                {
                    Ticker = ec.Ticker,
                    Exception = ExceptionType.DataNotProcessedTooLong,
                    ReportingDate = DateTime.UtcNow,
                    AdditionalNotes = $"{ec.Ticker} not processed  Yahoo: {ec.EarningsDateYahoo:yyyy-MM-dd} " +
                    $"FinnHub: {ec.VendorEarningsDate:yyyy-MM-dd}"
                });
            }
        }
        try
        {
            if (earningsCalExceptions.Any())
            {
                await exceptionRepository.Add(earningsCalExceptions);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database ExceptionReporting:DataNotProcessedTooLong");
            logger.LogError(ex.ToString());
            return false;
        }
    }

    private async Task<bool> DropAgedRecords()
    {
        try
        {
            var cutOffDate = DateTime.UtcNow.AddDays(-1 * daysToHoldReport);
            var agedRecords = await exceptionRepository.FindAll(x => x.ReportingDate <= cutOffDate);
            if (agedRecords.Any())
            {
                await exceptionRepository.Remove(agedRecords);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to delete aged records from database ExceptionReporting:DropAgedRecords");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> GenerateDateDiffReport()
    {
        IEnumerable<EarningsCalendar> ecContent;
        try
        {
            ecContent = await ecRepository.FindAll();
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read data from database ExceptionReporting:GenerateDateDiffReport");
            logger.LogError(ex.ToString());
            return false;
        }
        List<EarningsCalExceptions> earningsCalExceptions = new();
        foreach (var ec in ecContent)
        {
            int diffOfDates = (ec.VendorEarningsDate - ec.EarningsDateYahoo).Days;
            diffOfDates = Math.Abs(diffOfDates);
            if (diffOfDates >= YahooVsFinnHubDays)
            {
                earningsCalExceptions.Add(new()
                {
                    Ticker = ec.Ticker,
                    Exception = ExceptionType.VastDiffBetweenVendorDates,
                    ReportingDate = DateTime.UtcNow,
                    AdditionalNotes = $"Vendor: {ec.VendorEarningsDate:yyyy-MM-dd} Yahoo: " +
                    $"{ec.EarningsDateYahoo:yyyy-MM-dd}"
                });
            }
        }
        try
        {
            if (earningsCalExceptions.Any())
            {
                await exceptionRepository.Add(earningsCalExceptions);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database ExceptionReporting:GenerateDateDiffReport");
            logger.LogError(ex.ToString());
            return false;
        }
    }

    private async Task<bool> GenerateTickerNotInIndex()
    {
        List<string> tickers = new();
        IEnumerable<EarningsCalendar> ecContent = new List<EarningsCalendar>();
        try
        {
            tickers = (await idxRepository.FindAll()).Select(x => x.Ticker).ToList();
            ecContent = await ecRepository.FindAll();
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read data from database ExceptionReporting:GenerateTickerNotInIndex");
            logger.LogError(ex.ToString());
            return false;
        }

        //Ticker not in index
        List<EarningsCalExceptions> earningsCalExceptions = new();
        foreach (var ec in ecContent)
        {
            if (!tickers.Contains(ec.Ticker))
            {
                earningsCalExceptions.Add(new()
                {
                    Ticker = ec.Ticker,
                    Exception = ExceptionType.TickerNotInIndex,
                    ReportingDate = DateTime.UtcNow,
                    AdditionalNotes = $"{ec.Ticker} not in any index"
                });
            }
        }
        try
        {
            if (earningsCalExceptions.Any())
            {
                await exceptionRepository.Add(earningsCalExceptions);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database ExceptionReporting:GenerateTickerNotInIndex");
            logger.LogError(ex.ToString());
            return false;
        }
    }

    private async Task<bool> StaleRecordHeldInDatabase()
    {
        DateTime today = DateTime.UtcNow;
        IEnumerable<EarningsCalendar> ecContent;
        try
        {
            ecContent = (await ecRepository.FindAll(x => x.DataObtained == true))
                .Where(x => x.RemoveDate >= today);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to read data from database ExceptionReporting:StaleRecordHeldInDatabase");
            logger.LogError(ex.ToString());
            return false;
        }
        if (ecContent == null || !ecContent.Any())
        {
            return true;
        }
        List<EarningsCalExceptions> earningsCalExceptions = new();
        foreach (var item in ecContent)
        {
            earningsCalExceptions.Add(new()
            {
                Ticker = item.Ticker,
                ReportingDate = DateTime.UtcNow,
                Exception = ExceptionType.StaleRecordHeldInDatabase,
                AdditionalNotes = $"{item.Ticker} had to be purged before {item.RemoveDate:yyyy-MM-dd} today is {today:yyyy-MM-dd}"
            }); ;
        }
        try
        {
            if (earningsCalExceptions.Any())
            {
                await exceptionRepository.Add(earningsCalExceptions);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database ExceptionReporting:StaleRecordHeldInDatabase");
            logger.LogError(ex.ToString());
            return false;
        }
    }

    #endregion Private Methods
}