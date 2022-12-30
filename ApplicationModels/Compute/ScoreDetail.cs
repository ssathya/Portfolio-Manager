namespace ApplicationModels.Compute;

public class ScoreDetail : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public int PiotroskiComputedValue { get; set; }
    public DateTime LastEarningsDate { get; set; }
    public int SimFinRating { get; set; }
    public bool ReturnOnAssets { get; set; }
    public bool OperatingCashFlow { get; set; }
    public bool ChangeInROA { get; set; }
    public bool ChangInLongTermDebt { get; set; }
    public bool ReturnOnEquity { get; set; }
    public bool ChangeInNetIncome { get; set; }
    public bool ChangeInCurrentRatio { get; set; }
    public bool ChangeInSharesOutstanding { get; set; }
    public bool GrossMargin { get; set; }
}