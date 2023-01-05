using Newtonsoft.Json;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class BSReport
{
    [JsonProperty("fiscalDateEnding")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; } = string.Empty;

    [JsonProperty("totalAssets")]
    public decimal? TotalAssets { get; set; }

    [JsonProperty("totalCurrentAssets")]
    public decimal? TotalCurrentAssets { get; set; }

    [JsonProperty("cashAndCashEquivalentsAtCarryingValue")]
    public decimal? CashAndCashEquivalentsAtCarryingValue { get; set; }

    [JsonProperty("cashAndShortTermInvestments")]
    public decimal? CashAndShortTermInvestments { get; set; }

    [JsonProperty("inventory")]
    public decimal? Inventory { get; set; }

    [JsonProperty("currentNetReceivables")]
    public decimal? CurrentNetReceivables { get; set; }

    [JsonProperty("totalNonCurrentAssets")]
    public decimal? TotalNonCurrentAssets { get; set; }

    [JsonProperty("propertyPlantEquipment")]
    public decimal? PropertyPlantEquipment { get; set; }

    [JsonProperty("accumulatedDepreciationAmortizationPPE")]
    public decimal? AccumulatedDepreciationAmortizationPPE { get; set; }

    [JsonProperty("intangibleAssets")]
    public decimal? IntangibleAssets { get; set; }

    [JsonProperty("intangibleAssetsExcludingGoodwill")]
    public decimal? IntangibleAssetsExcludingGoodwill { get; set; }

    [JsonProperty("goodwill")]
    public decimal? Goodwill { get; set; }

    [JsonProperty("investments")]
    public decimal? Investments { get; set; }

    [JsonProperty("longTermInvestments")]
    public decimal? LongTermInvestments { get; set; }

    [JsonProperty("shortTermInvestments")]
    public decimal? ShortTermInvestments { get; set; }

    [JsonProperty("otherCurrentAssets")]
    public decimal? OtherCurrentAssets { get; set; }

    [JsonProperty("otherNonCurrrentAssets")]
    public decimal? OtherNonCurrrentAssets { get; set; }

    [JsonProperty("totalLiabilities")]
    public decimal? TotalLiabilities { get; set; }

    [JsonProperty("totalCurrentLiabilities")]
    public decimal? TotalCurrentLiabilities { get; set; }

    [JsonProperty("currentAccountsPayable")]
    public decimal? CurrentAccountsPayable { get; set; }

    [JsonProperty("deferredRevenue")]
    public decimal? DeferredRevenue { get; set; }

    [JsonProperty("currentDebt")]
    public decimal? CurrentDebt { get; set; }

    [JsonProperty("shortTermDebt")]
    public decimal? ShortTermDebt { get; set; }

    [JsonProperty("totalNonCurrentLiabilities")]
    public decimal? TotalNonCurrentLiabilities { get; set; }

    [JsonProperty("capitalLeaseObligations")]
    public decimal? CapitalLeaseObligations { get; set; }

    [JsonProperty("longTermDebt")]
    public decimal? LongTermDebt { get; set; }

    [JsonProperty("currentLongTermDebt")]
    public decimal? CurrentLongTermDebt { get; set; }

    [JsonProperty("longTermDebtNoncurrent")]
    public decimal? LongTermDebtNoncurrent { get; set; }

    [JsonProperty("shortLongTermDebtTotal")]
    public decimal? ShortLongTermDebtTotal { get; set; }

    [JsonProperty("otherCurrentLiabilities")]
    public decimal? OtherCurrentLiabilities { get; set; }

    [JsonProperty("otherNonCurrentLiabilities")]
    public decimal? OtherNonCurrentLiabilities { get; set; }

    [JsonProperty("totalShareholderEquity")]
    public decimal? TotalShareholderEquity { get; set; }

    [JsonProperty("treasuryStock")]
    public decimal? TreasuryStock { get; set; }

    [JsonProperty("retainedEarnings")]
    public decimal? RetainedEarnings { get; set; }

    [JsonProperty("commonStock")]
    public decimal? CommonStock { get; set; }

    [JsonProperty("commonStockSharesOutstanding")]
    public decimal? CommonStockSharesOutstanding { get; set; }
}