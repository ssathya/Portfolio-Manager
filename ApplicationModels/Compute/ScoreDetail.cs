namespace ApplicationModels.Compute;

public class ScoreDetail : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public int PiotroskiComputedValue { get; set; }
    public DateTime LastEarningsDate { get; set; }
    public int SimFinRating { get; set; }

    //From Other sources
    //Profitability
    public bool ReturnOnAssets { get; set; }

    public bool OperatingCashFlow { get; set; }
    public bool IsROABetter { get; set; }
    public bool Accruals { get; set; }

    //Leverage/Liquidity
    public bool ChangeInLeverage { get; set; }

    public bool ChangeInCurrentRatio { get; set; }
    public bool ChangeInNumberOfShares { get; set; }

    //Operating Efficiency
    public bool IncreaseGrossMargin { get; set; }

    public bool AssetTurnoverRatio { get; set; }
}