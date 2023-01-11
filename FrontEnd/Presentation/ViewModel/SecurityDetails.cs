using ApplicationModels.Indexes;

namespace Presentation.ViewModel;

public class SecurityDetails
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public IndexNames ListedInIndex { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string SubSector { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public bool ListedInSnP { get; set; }
    public bool ListedInNasdaq { get; set; }
    public bool ListedInDow { get; set; }

    public static implicit operator SecurityDetails(IndexComponent ic)
    {
        return new SecurityDetails
        {
            Id = ic.Id,
            CompanyName = ic.CompanyName,
            Sector = ic.Sector,
            SubSector = ic.SubSector,
            Ticker = ic.Ticker,
            ListedInIndex = ic.ListedInIndex,
            ListedInSnP = ic.ListedInIndex.HasFlag(IndexNames.SnP),
            ListedInDow = ic.ListedInIndex.HasFlag(IndexNames.Dow),
            ListedInNasdaq = ic.ListedInIndex.HasFlag(IndexNames.Nasdaq)
        };
    }
}