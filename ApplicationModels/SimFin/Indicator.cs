namespace ApplicationModels.SimFin;

public class Indicator
{
    public string IndicatorId { get; set; } = string.Empty;
    public string IndicatorName { get; set; } = String.Empty;
    public decimal Value { get; set; }
    public string Period { get; set; } = string.Empty;
    public int? Fyear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? PeriodEndDate { get; set; }
}