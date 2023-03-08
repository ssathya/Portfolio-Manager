using Newtonsoft.Json;
using OfficeOpenXml.Attributes;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class BSReport
{
    [JsonProperty("fiscalDateEnding")]
    [EpplusTableColumn(NumberFormat = "MM-dd-yyyy")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; } = string.Empty;

    [JsonProperty("totalAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalAssets { get; set; }

    [JsonProperty("totalCurrentAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalCurrentAssets { get; set; }

    [JsonProperty("cashAndCashEquivalentsAtCarryingValue")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CashAndCashEquivalentsAtCarryingValue { get; set; }

    [JsonProperty("cashAndShortTermInvestments")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CashAndShortTermInvestments { get; set; }

    [JsonProperty("inventory")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Inventory { get; set; }

    [JsonProperty("currentNetReceivables")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CurrentNetReceivables { get; set; }

    [JsonProperty("totalNonCurrentAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalNonCurrentAssets { get; set; }

    [JsonProperty("propertyPlantEquipment")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? PropertyPlantEquipment { get; set; }

    [JsonProperty("accumulatedDepreciationAmortizationPPE")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? AccumulatedDepreciationAmortizationPPE { get; set; }

    [JsonProperty("intangibleAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? IntangibleAssets { get; set; }

    [JsonProperty("intangibleAssetsExcludingGoodwill")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? IntangibleAssetsExcludingGoodwill { get; set; }

    [JsonProperty("goodwill")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Goodwill { get; set; }

    [JsonProperty("investments")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Investments { get; set; }

    [JsonProperty("longTermInvestments")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? LongTermInvestments { get; set; }

    [JsonProperty("shortTermInvestments")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ShortTermInvestments { get; set; }

    [JsonProperty("otherCurrentAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OtherCurrentAssets { get; set; }

    [JsonProperty("otherNonCurrrentAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OtherNonCurrrentAssets { get; set; }

    [JsonProperty("totalLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalLiabilities { get; set; }

    [JsonProperty("totalCurrentLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalCurrentLiabilities { get; set; }

    [JsonProperty("currentAccountsPayable")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CurrentAccountsPayable { get; set; }

    [JsonProperty("deferredRevenue")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DeferredRevenue { get; set; }

    [JsonProperty("currentDebt")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CurrentDebt { get; set; }

    [JsonProperty("shortTermDebt")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ShortTermDebt { get; set; }

    [JsonProperty("totalNonCurrentLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalNonCurrentLiabilities { get; set; }

    [JsonProperty("capitalLeaseObligations")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CapitalLeaseObligations { get; set; }

    [JsonProperty("longTermDebt")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? LongTermDebt { get; set; }

    [JsonProperty("currentLongTermDebt")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CurrentLongTermDebt { get; set; }

    [JsonProperty("longTermDebtNoncurrent")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? LongTermDebtNoncurrent { get; set; }

    [JsonProperty("shortLongTermDebtTotal")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ShortLongTermDebtTotal { get; set; }

    [JsonProperty("otherCurrentLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OtherCurrentLiabilities { get; set; }

    [JsonProperty("otherNonCurrentLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OtherNonCurrentLiabilities { get; set; }

    [JsonProperty("totalShareholderEquity")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalShareholderEquity { get; set; }

    [JsonProperty("treasuryStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TreasuryStock { get; set; }

    [JsonProperty("retainedEarnings")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? RetainedEarnings { get; set; }

    [JsonProperty("commonStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CommonStock { get; set; }

    [JsonProperty("commonStockSharesOutstanding")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CommonStockSharesOutstanding { get; set; }
}