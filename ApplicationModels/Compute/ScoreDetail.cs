namespace ApplicationModels.Compute;

public class ScoreDetail : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public int PiotroskiComputedValue { get; set; }
    public DateTime LastEarningsDate { get; set; }
    public int SimFinRating { get; set; }

    //From Other sources
    //Profitability
    public bool NetIncome { get; set; }

    public bool OperatingCashFlow { get; set; }
    public bool ReturnOnAssets { get; set; }
    public bool QualityOfEarnings { get; set; }

    //Leverage/Liquidity
    public bool DecreasedLeverage { get; set; }

    public bool IncreasedLiquidity { get; set; }
    public bool ZeroDilution { get; set; }

    //Operating Efficiency
    public bool GrossMargin { get; set; }

    public bool AssetTurnOver { get; set; }
}