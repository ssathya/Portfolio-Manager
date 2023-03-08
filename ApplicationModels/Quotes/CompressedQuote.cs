using Skender.Stock.Indicators;

namespace ApplicationModels.Quotes;

public class CompressedQuote
{
    public DateTime Date { get; set; }
    public decimal ClosingPrice { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public long Volume { get; set; }

    public static implicit operator Quote(CompressedQuote cq)
    {
        return new Quote
        {
            Date = cq.Date,
            Open = cq.Open,
            High = cq.High,
            Low = cq.Low,
            Volume = cq.Volume,
            Close = cq.ClosingPrice
        };
    }
}