using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.Quotes;

public class YPrice : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime UpdateDate { get; set; }

    [Column(TypeName = "jsonb")]
    public List<CompressedQuote> CompressedQuotes { get; set; } = new();
}