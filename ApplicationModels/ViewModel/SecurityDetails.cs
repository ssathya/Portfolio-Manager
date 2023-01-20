using ApplicationModels.Indexes;
using ApplicationModels.Views;
using OfficeOpenXml.Attributes;

namespace ApplicationModels.ViewModel;

public class SecurityDetails
{
    [EpplusIgnore]
    public int Id { get; set; }

    [EpplusTableColumn(Header = "Name")]
    public string CompanyName { get; set; } = string.Empty;

    [EpplusIgnore]
    public IndexNames ListedInIndex { get; set; }

    [EpplusTableColumn(Header = "Sector")]
    public string Sector { get; set; } = string.Empty;

    [EpplusTableColumn(Header = "Sub Sector")]
    public string SubSector { get; set; } = string.Empty;

    [EpplusTableColumn(Header = "Ticker")]
    public string Ticker { get; set; } = string.Empty;

    [EpplusTableColumn(Header = "SnP-500")]
    public bool ListedInSnP { get; set; }

    [EpplusTableColumn(Header = "NASDAQ-100")]
    public bool ListedInNasdaq { get; set; }

    [EpplusTableColumn(Header = "Dow-30")]
    public bool ListedInDow { get; set; }

    [EpplusTableColumn(Header = "Piotroski - Score")]
    public int PiotroskiComputedValue { get; set; }

    [EpplusTableColumn(Header = "Sim Fin Ratings")]
    public int SimFinRating { get; set; }

    [EpplusTableColumn(Header = "Dollar Volume", NumberFormat = "#,##0.00")]
    public decimal DollarVolume { get; set; }

    [EpplusTableColumn(Header = "Momentum")]
    public decimal Momentum { get; set; }

    [EpplusTableColumn(Header = "Money Flow", NumberFormat = "0.00")]
    public decimal MoneyFlow { get; set; }

    [EpplusIgnore]
    public static implicit operator SecurityDetails(SecurityWithPScore ic)
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
            ListedInNasdaq = ic.ListedInIndex.HasFlag(IndexNames.Nasdaq),
            PiotroskiComputedValue = ic.PiotroskiComputedValue ?? 0,
            SimFinRating = ic.SimFinRating ?? 0
        };
    }
}