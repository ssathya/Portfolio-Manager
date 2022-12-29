namespace ApplicationModels.Quotes;

public class CompressedQuote
{
    public DateTime Date { get; set; }
    public decimal ClosingPrice { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public long Volume { get; set; }
}