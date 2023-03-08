using Skender.Stock.Indicators;

namespace ApplicationModels.Compute;

public class ComputedValues
{
    public DateTime ReportingDate { get; set; }
    public decimal MomentumValue { get; set; }
    public decimal MoneyFlow { get; set; }

    //public RocResult? RocResult { get; set; }
    public decimal Roc { get; set; }

    public decimal Sma { get; set; }
}