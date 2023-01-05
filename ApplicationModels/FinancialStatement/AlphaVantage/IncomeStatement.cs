using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class IncomeStatement : Entity
{
    [JsonProperty("symbol")]
    public string Ticker { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<IncomeReport> AnnualReports { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<IncomeReport> QuarterlyReports { get; set; } = new();
}