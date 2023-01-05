using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class CashFlow : Entity
{
    [JsonProperty("symbol")]
    public string Ticker { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<CashFlowReport> AnnualReports { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<CashFlowReport> QuarterlyReports { get; set; } = new();
}