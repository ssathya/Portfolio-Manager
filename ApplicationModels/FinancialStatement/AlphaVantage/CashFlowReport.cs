using Newtonsoft.Json;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class CashFlowReport
{
    [JsonProperty("fiscalDateEnding")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; }= string.Empty;

    [JsonProperty("operatingCashflow")]
    public decimal? OperatingCashflow { get; set; }

    [JsonProperty("paymentsForOperatingActivities")]
    public decimal? PaymentsForOperatingActivities { get; set; }

    [JsonProperty("proceedsFromOperatingActivities")]
    public decimal? ProceedsFromOperatingActivities { get; set; }

    [JsonProperty("changeInOperatingLiabilities")]
    public decimal? ChangeInOperatingLiabilities { get; set; }

    [JsonProperty("changeInOperatingAssets")]
    public decimal? ChangeInOperatingAssets { get; set; }

    [JsonProperty("depreciationDepletionAndAmortization")]
    public decimal? DepreciationDepletionAndAmortization { get; set; }

    [JsonProperty("capitalExpenditures")]
    public decimal? CapitalExpenditures { get; set; }

    [JsonProperty("changeInReceivables")]
    public decimal? ChangeInReceivables { get; set; }

    [JsonProperty("changeInInventory")]
    public decimal? ChangeInInventory { get; set; }

    [JsonProperty("profitLoss")]
    public decimal? ProfitLoss { get; set; }

    [JsonProperty("cashflowFromInvestment")]
    public decimal? CashflowFromInvestment { get; set; }

    [JsonProperty("cashflowFromFinancing")]
    public decimal? CashflowFromFinancing { get; set; }

    [JsonProperty("proceedsFromRepaymentsOfShortTermDebt")]
    public decimal? ProceedsFromRepaymentsOfShortTermDebt { get; set; }

    [JsonProperty("paymentsForRepurchaseOfCommonStock")]
    public decimal? PaymentsForRepurchaseOfCommonStock { get; set; }

    [JsonProperty("paymentsForRepurchaseOfEquity")]
    public decimal? PaymentsForRepurchaseOfEquity { get; set; }

    [JsonProperty("paymentsForRepurchaseOfPreferredStock")]
    public decimal? PaymentsForRepurchaseOfPreferredStock { get; set; }

    [JsonProperty("dividendPayout")]
    public decimal? DividendPayout { get; set; }

    [JsonProperty("dividendPayoutCommonStock")]
    public decimal? DividendPayoutCommonStock { get; set; }

    [JsonProperty("dividendPayoutPreferredStock")]
    public decimal? DividendPayoutPreferredStock { get; set; }

    [JsonProperty("proceedsFromIssuanceOfCommonStock")]
    public decimal? ProceedsFromIssuanceOfCommonStock { get; set; }

    [JsonProperty("proceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet")]
    public decimal? ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet { get; set; }

    [JsonProperty("proceedsFromIssuanceOfPreferredStock")]
    public decimal? ProceedsFromIssuanceOfPreferredStock { get; set; }

    [JsonProperty("proceedsFromRepurchaseOfEquity")]
    public decimal? ProceedsFromRepurchaseOfEquity { get; set; }

    [JsonProperty("proceedsFromSaleOfTreasuryStock")]
    public decimal? ProceedsFromSaleOfTreasuryStock { get; set; }

    [JsonProperty("changeInCashAndCashEquivalents")]
    public decimal? ChangeInCashAndCashEquivalents { get; set; }

    [JsonProperty("changeInExchangeRate")]
    public decimal? ChangeInExchangeRate { get; set; }

    [JsonProperty("netIncome")]
    public decimal? NetIncome { get; set; }
}