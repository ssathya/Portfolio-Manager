using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.SimFin;

public class SimFinRatio : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime ProcessingDate { get; set; }

    [Column(TypeName = "jsonb")]
    public List<Indicator> Indicators { get; set; } = new();
}