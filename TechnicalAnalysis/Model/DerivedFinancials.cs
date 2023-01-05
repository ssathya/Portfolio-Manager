namespace TechnicalAnalysis.Model;

public class DerivedFinancials
{
    public string Symbol { get; set; } = string.Empty;
    public DateTime LastFiscalDateEnding { get; set; }

    //Balance sheet values
    public decimal LongTermDebt { get; set; }

    public decimal TotalAssets { get; set; }
    public decimal CurrentAssets { get; set; }
    public decimal CurrentLiabilities { get; set; }
    public decimal PyLongTermDebt { get; set; }
    public decimal PyTotalAssets { get; set; }
    public decimal PyCurrentAssets { get; set; }
    public decimal PyCurrentLiabilities { get; set; }

    public decimal PyPyTotalAssets { get; set; }

    public decimal WaSharesOutstanding { get; set; }
    public decimal PyWaSharesOutstanding { get; set; }

    //Income statement
    public decimal CyRevenue { get; set; }

    public decimal PyRevenue { get; set; }
    public decimal CyGrossProfit { get; set; }
    public decimal PyGrossProfit { get; set; }
    public decimal CyNetIncome { get; set; }
    public decimal PyNetIncome { get; set; }

    //Cash-flow
    public decimal OperatingCashFlow { get; set; }
}