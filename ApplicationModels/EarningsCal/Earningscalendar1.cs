namespace ApplicationModels.EarningsCal;

public class Earningscalendar
{
    public DateTime Date { get; set; }
    public decimal? EpsActual { get; set; }
    public decimal? EpsEstimate { get; set; }
    public string? Hour { get; set; }
    public int? Quarter { get; set; }
    public long? RevenueActual { get; set; }
    public long? RevenueEstimate { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int? Year { get; set; }
}