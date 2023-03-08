using Newtonsoft.Json;
using OfficeOpenXml.Attributes;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class IncomeReport
{
    [JsonProperty("fiscalDateEnding")]
    [EpplusTableColumn(NumberFormat = "MM-dd-yyyy")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; } = string.Empty;

    [JsonProperty("grossProfit")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? GrossProfit { get; set; }

    [JsonProperty("totalRevenue")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? TotalRevenue { get; set; }

    [JsonProperty("costOfRevenue")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CostOfRevenue { get; set; }

    [JsonProperty("costofGoodsAndServicesSold")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? CostofGoodsAndServicesSold { get; set; }

    [JsonProperty("operatingIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OperatingIncome { get; set; }

    [JsonProperty("sellingGeneralAndAdministrative")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? SellingGeneralAndAdministrative { get; set; }

    [JsonProperty("researchAndDevelopment")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ResearchAndDevelopment { get; set; }

    [JsonProperty("operatingExpenses")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OperatingExpenses { get; set; }

    [JsonProperty("investmentIncomeNet")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? InvestmentIncomeNet { get; set; }

    [JsonProperty("netInterestIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? NetInterestIncome { get; set; }

    [JsonProperty("interestIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? InterestIncome { get; set; }

    [JsonProperty("interestExpense")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? InterestExpense { get; set; }

    [JsonProperty("nonInterestIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? NonInterestIncome { get; set; }

    [JsonProperty("otherNonOperatingIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? OtherNonOperatingIncome { get; set; }

    [JsonProperty("depreciation")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Depreciation { get; set; }

    [JsonProperty("depreciationAndAmortization")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? DepreciationAndAmortization { get; set; }

    [JsonProperty("incomeBeforeTax")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? IncomeBeforeTax { get; set; }

    [JsonProperty("incomeTaxExpense")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? IncomeTaxExpense { get; set; }

    [JsonProperty("interestAndDebtExpense")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? InterestAndDebtExpense { get; set; }

    [JsonProperty("netIncomeFromContinuingOperations")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? NetIncomeFromContinuingOperations { get; set; }

    [JsonProperty("comprehensiveIncomeNetOfTax")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? ComprehensiveIncomeNetOfTax { get; set; }

    [JsonProperty("ebit")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Ebit { get; set; }

    [JsonProperty("ebitda")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? Ebitda { get; set; }

    [JsonProperty("netIncome")]
    [EpplusTableColumn(NumberFormat = "#,##0")]
    public decimal? NetIncome { get; set; }
}