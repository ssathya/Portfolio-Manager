using Newtonsoft.Json;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class IncomeReport
{
    [JsonProperty("fiscalDateEnding")]
    public DateTime FiscalDateEnding { get; set; }

    [JsonProperty("reportedCurrency")]
    public string ReportedCurrency { get; set; }

    [JsonProperty("grossProfit")]
    public decimal? GrossProfit { get; set; }

    [JsonProperty("totalRevenue")]
    public decimal? TotalRevenue { get; set; }

    [JsonProperty("costOfRevenue")]
    public decimal? CostOfRevenue { get; set; }

    [JsonProperty("costofGoodsAndServicesSold")]
    public decimal? CostofGoodsAndServicesSold { get; set; }

    [JsonProperty("operatingIncome")]
    public decimal? OperatingIncome { get; set; }

    [JsonProperty("sellingGeneralAndAdministrative")]
    public decimal? SellingGeneralAndAdministrative { get; set; }

    [JsonProperty("researchAndDevelopment")]
    public decimal? ResearchAndDevelopment { get; set; }

    [JsonProperty("operatingExpenses")]
    public decimal? OperatingExpenses { get; set; }

    [JsonProperty("investmentIncomeNet")]
    public decimal? InvestmentIncomeNet { get; set; }

    [JsonProperty("netInterestIncome")]
    public decimal? NetInterestIncome { get; set; }

    [JsonProperty("interestIncome")]
    public decimal? InterestIncome { get; set; }

    [JsonProperty("interestExpense")]
    public decimal? InterestExpense { get; set; }

    [JsonProperty("nonInterestIncome")]
    public decimal? NonInterestIncome { get; set; }

    [JsonProperty("otherNonOperatingIncome")]
    public decimal? OtherNonOperatingIncome { get; set; }

    [JsonProperty("depreciation")]
    public decimal? Depreciation { get; set; }

    [JsonProperty("depreciationAndAmortization")]
    public decimal? DepreciationAndAmortization { get; set; }

    [JsonProperty("incomeBeforeTax")]
    public decimal? IncomeBeforeTax { get; set; }

    [JsonProperty("incomeTaxExpense")]
    public decimal? IncomeTaxExpense { get; set; }

    [JsonProperty("interestAndDebtExpense")]
    public decimal? InterestAndDebtExpense { get; set; }

    [JsonProperty("netIncomeFromContinuingOperations")]
    public decimal? NetIncomeFromContinuingOperations { get; set; }

    [JsonProperty("comprehensiveIncomeNetOfTax")]
    public decimal? ComprehensiveIncomeNetOfTax { get; set; }

    [JsonProperty("ebit")]
    public decimal? Ebit { get; set; }

    [JsonProperty("ebitda")]
    public decimal? Ebitda { get; set; }

    [JsonProperty("netIncome")]
    public decimal? NetIncome { get; set; }
}