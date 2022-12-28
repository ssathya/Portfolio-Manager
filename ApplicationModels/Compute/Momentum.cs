using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.Compute;

public class Momentum : Entity
{
    public string Ticker { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<ComputedValues> ComputedValues { get; set; } = new();
}