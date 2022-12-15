using System.ComponentModel.DataAnnotations;

namespace ApplicationModels.EarningsCal;

public class EarningsCalendar : Entity
{
    [Required]
    public string Ticker { get; set; } = string.Empty;

    public DateTime EarningsDateYahoo { get; set; } = new DateTime(1900, 1, 1);
    public DateTime VendorEarningsDate { get; set; } = new DateTime(1900, 1, 1);
    public DateTime EarningsReadDate { get; set; } = new DateTime(1900, 1, 1);
    public DateTime RemoveDate { get; set; } = new DateTime(2100, 1, 1);
    public bool DataObtained { get; set; } = false;
}