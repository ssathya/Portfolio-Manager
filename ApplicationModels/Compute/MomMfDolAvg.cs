namespace ApplicationModels.Compute;

/// <summary>
/// Momentum, dollar volume average, money flow
/// </summary>
/// <seealso cref="ApplicationModels.Entity" />
public class MomMfDolAvg : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Momentum { get; set; }
    public decimal DollarVolume { get; set; }
    public decimal MoneyFlow { get; set; }
    public decimal SmA { get; set; }
}