using Newtonsoft.Json;

namespace ApplicationModels.FinancialStatement.AlphaVantage;

public class Overview : Entity
{
    [JsonProperty("Symbol")]
    public string Ticker { get; set; } = string.Empty;

    [JsonProperty("AssetType")]
    public string AssetType { get; set; } = string.Empty;

    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("CIK")]
    public string CIK { get; set; } = string.Empty;

    [JsonProperty("Exchange")]
    public string Exchange { get; set; } = string.Empty;

    [JsonProperty("Currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("Country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("Sector")]
    public string Sector { get; set; } = string.Empty;

    [JsonProperty("Industry")]
    public string Industry { get; set; } = string.Empty;

    [JsonProperty("Address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("FiscalYearEnd")]
    public string FiscalYearEnd { get; set; } = string.Empty;

    [JsonProperty("LatestQuarter")]
    public DateTime? LatestQuarter { get; set; }

    [JsonProperty("MarketCapitalization")]
    public decimal? MarketCapitalization { get; set; }

    [JsonProperty("EBITDA")]
    public decimal? EBITDA { get; set; }

    [JsonProperty("PERatio")]
    public decimal? PERatio { get; set; }

    [JsonProperty("PEGRatio")]
    public decimal? PEGRatio { get; set; }

    [JsonProperty("BookValue")]
    public decimal? BookValue { get; set; }

    [JsonProperty("DividendPerShare")]
    public decimal? DividendPerShare { get; set; }

    [JsonProperty("DividendYield")]
    public decimal? DividendYield { get; set; }

    [JsonProperty("EPS")]
    public decimal? EPS { get; set; }

    [JsonProperty("RevenuePerShareTTM")]
    public decimal? RevenuePerShareTTM { get; set; }

    [JsonProperty("ProfitMargin")]
    public decimal? ProfitMargin { get; set; }

    [JsonProperty("OperatingMarginTTM")]
    public decimal? OperatingMarginTTM { get; set; }

    [JsonProperty("ReturnOnAssetsTTM")]
    public decimal? ReturnOnAssetsTTM { get; set; }

    [JsonProperty("ReturnOnEquityTTM")]
    public decimal? ReturnOnEquityTTM { get; set; }

    [JsonProperty("RevenueTTM")]
    public decimal? RevenueTTM { get; set; }

    [JsonProperty("GrossProfitTTM")]
    public decimal? GrossProfitTTM { get; set; }

    [JsonProperty("DilutedEPSTTM")]
    public decimal? DilutedEPSTTM { get; set; }

    [JsonProperty("QuarterlyEarningsGrowthYOY")]
    public decimal? QuarterlyEarningsGrowthYOY { get; set; }

    [JsonProperty("QuarterlyRevenueGrowthYOY")]
    public decimal? QuarterlyRevenueGrowthYOY { get; set; }

    [JsonProperty("AnalystTargetPrice")]
    public decimal? AnalystTargetPrice { get; set; }

    [JsonProperty("TrailingPE")]
    public decimal? TrailingPE { get; set; }

    [JsonProperty("ForwardPE")]
    public decimal? ForwardPE { get; set; }

    [JsonProperty("PriceToSalesRatioTTM")]
    public decimal? PriceToSalesRatioTTM { get; set; }

    [JsonProperty("PriceToBookRatio")]
    public decimal? PriceToBookRatio { get; set; }

    [JsonProperty("EVToRevenue")]
    public decimal? EVToRevenue { get; set; }

    [JsonProperty("EVToEBITDA")]
    public decimal? EVToEBITDA { get; set; }

    [JsonProperty("Beta")]
    public decimal? Beta { get; set; }

    [JsonProperty("52WeekHigh")]
    public decimal? _52WeekHigh { get; set; }

    [JsonProperty("52WeekLow")]
    public decimal? _52WeekLow { get; set; }

    [JsonProperty("50DayMovingAverage")]
    public decimal? _50DayMovingAverage { get; set; }

    [JsonProperty("200DayMovingAverage")]
    public decimal? _200DayMovingAverage { get; set; }

    [JsonProperty("SharesOutstanding")]
    public decimal? SharesOutstanding { get; set; }

    [JsonProperty("SharesFloat")]
    public decimal? SharesFloat { get; set; }

    [JsonProperty("SharesShort")]
    public decimal? SharesShort { get; set; }

    [JsonProperty("SharesShortPriorMonth")]
    public decimal? SharesShortPriorMonth { get; set; }

    [JsonProperty("ShortRatio")]
    public decimal? ShortRatio { get; set; }

    [JsonProperty("ShortPercentOutstanding")]
    public decimal? ShortPercentOutstanding { get; set; }

    [JsonProperty("ShortPercentFloat")]
    public decimal? ShortPercentFloat { get; set; }

    [JsonProperty("PercentInsiders")]
    public decimal? PercentInsiders { get; set; }

    [JsonProperty("PercentInstitutions")]
    public decimal? PercentInstitutions { get; set; }

    [JsonProperty("ForwardAnnualDividendRate")]
    public decimal? ForwardAnnualDividendRate { get; set; }

    [JsonProperty("ForwardAnnualDividendYield")]
    public decimal? ForwardAnnualDividendYield { get; set; }

    [JsonProperty("PayoutRatio")]
    public decimal? PayoutRatio { get; set; }

    [JsonProperty("DividendDate")]
    public DateTime? DividendDate { get; set; }

    [JsonProperty("ExDividendDate")]
    public DateTime? ExDividendDate { get; set; }

    [JsonProperty("LastSplitFactor")]
    public string LastSplitFactor { get; set; } = string.Empty;

    [JsonProperty("LastSplitDate")]
    public DateTime? LastSplitDate { get; set; }
}