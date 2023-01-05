using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class BalanceSheet : Entity
{
    [JsonProperty("symbol")]
    public string Ticker { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<BSReport> AnnualReports { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<BSReport> QuarterlyReports { get; set; } = new();
}