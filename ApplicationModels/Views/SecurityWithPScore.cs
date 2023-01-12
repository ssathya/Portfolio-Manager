using ApplicationModels.Indexes;

namespace ApplicationModels.Views;

public class SecurityWithPScore : Entity
{
    public string Ticker { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public IndexNames ListedInIndex { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string SubSector { get; set; } = string.Empty;
    public int? PiotroskiComputedValue { get; set; }
    public int? SimFinRating { get; set; }
}