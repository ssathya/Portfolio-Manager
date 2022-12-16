using System.ComponentModel.DataAnnotations;

namespace ApplicationModels.EarningsCal;

public class EarningsCalExceptions : Entity
{
    [Required]
    public string Ticker { get; set; } = string.Empty;

    public ExceptionType Exception { get; set; }
    public DateTime ReportingDate { get; set; }
    public string? AdditionalNotes { get; set; }
}

public enum ExceptionType
{
    None = 0,
    VastDiffBetweenVendorDates = 1,
    StaleRecordHeldInDatabase = 2,
    DataNotProcessedTooLong = 3,
    TickerNotInIndex = 4
}