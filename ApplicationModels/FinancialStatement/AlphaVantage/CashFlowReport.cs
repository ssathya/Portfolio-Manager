using Newtonsoft.Json;
using OfficeOpenXml.Attributes;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class CashFlowReport
{
    [JsonProperty("fiscalDateEnding")]
    [EpplusTableColumn(NumberFormat = "MM-dd-yyyy")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; } = string.Empty;

    [JsonProperty("operatingCashflow")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OperatingCashflow { get; set; }

    [JsonProperty("paymentsForOperatingActivities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? PaymentsForOperatingActivities { get; set; }

    [JsonProperty("proceedsFromOperatingActivities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromOperatingActivities { get; set; }

    [JsonProperty("changeInOperatingLiabilities")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInOperatingLiabilities { get; set; }

    [JsonProperty("changeInOperatingAssets")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInOperatingAssets { get; set; }

    [JsonProperty("depreciationDepletionAndAmortization")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DepreciationDepletionAndAmortization { get; set; }

    [JsonProperty("capitalExpenditures")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CapitalExpenditures { get; set; }

    [JsonProperty("changeInReceivables")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInReceivables { get; set; }

    [JsonProperty("changeInInventory")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInInventory { get; set; }

    [JsonProperty("profitLoss")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProfitLoss { get; set; }

    [JsonProperty("cashflowFromInvestment")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CashflowFromInvestment { get; set; }

    [JsonProperty("cashflowFromFinancing")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CashflowFromFinancing { get; set; }

    [JsonProperty("proceedsFromRepaymentsOfShortTermDebt")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromRepaymentsOfShortTermDebt { get; set; }

    [JsonProperty("paymentsForRepurchaseOfCommonStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? PaymentsForRepurchaseOfCommonStock { get; set; }

    [JsonProperty("paymentsForRepurchaseOfEquity")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? PaymentsForRepurchaseOfEquity { get; set; }

    [JsonProperty("paymentsForRepurchaseOfPreferredStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? PaymentsForRepurchaseOfPreferredStock { get; set; }

    [JsonProperty("dividendPayout")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DividendPayout { get; set; }

    [JsonProperty("dividendPayoutCommonStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DividendPayoutCommonStock { get; set; }

    [JsonProperty("dividendPayoutPreferredStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DividendPayoutPreferredStock { get; set; }

    [JsonProperty("proceedsFromIssuanceOfCommonStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromIssuanceOfCommonStock { get; set; }

    [JsonProperty("proceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet { get; set; }

    [JsonProperty("proceedsFromIssuanceOfPreferredStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromIssuanceOfPreferredStock { get; set; }

    [JsonProperty("proceedsFromRepurchaseOfEquity")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromRepurchaseOfEquity { get; set; }

    [JsonProperty("proceedsFromSaleOfTreasuryStock")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ProceedsFromSaleOfTreasuryStock { get; set; }

    [JsonProperty("changeInCashAndCashEquivalents")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInCashAndCashEquivalents { get; set; }

    [JsonProperty("changeInExchangeRate")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ChangeInExchangeRate { get; set; }

    [JsonProperty("netIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? NetIncome { get; set; }
}