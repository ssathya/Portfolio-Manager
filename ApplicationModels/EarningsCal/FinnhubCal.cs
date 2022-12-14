namespace ApplicationModels.EarningsCal;

public class FinnhubCal
{
    #region Public Properties

    public Earningscalendar[]? EarningsCalendar { get; set; }

    #endregion Public Properties
}

public class Earningscalendar
{
    public string? Date { get; set; }
    public decimal? EpsActual { get; set; }
    public decimal? EpsEstimate { get; set; }
    public string? Hour { get; set; }
    public int? Quarter { get; set; }
    public long? RevenueActual { get; set; }
    public long? RevenueEstimate { get; set; }
    public string? Symbol { get; set; }
    public int? Year { get; set; }
}