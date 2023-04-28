using Skender.Stock.Indicators;

namespace Presentation.Models;

public class ChartingValues
{
    public DateTime Date { get; set; }
    public double? Value { get; set; }

    public static implicit operator ChartingValues(SmaResult r)
    {
        return new ChartingValues
        {
            Date = r.Date,
            Value = r.Sma
        };
    }

    public static implicit operator ChartingValues(EmaResult r)
    {
        return new ChartingValues
        {
            Date = r.Date,
            Value = r.Ema
        };
    }
}