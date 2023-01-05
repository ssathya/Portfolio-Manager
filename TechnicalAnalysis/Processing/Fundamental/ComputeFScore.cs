using TechnicalAnalysis.Model;

namespace TechnicalAnalysis.Processing.Fundamental;

public static class ComputeFScore
{
    #region Public Methods

    public static int ComputeScores(DerivedFinancials df)
    {
        int score = 0;
        score += Compute1ReturnOnAssets(df);
        score += Compute2OperatingCashFlow(df);
        score += Compute3IsROABetter(df);
        score += Compute4Accruals(df);
        score += Compute5ChangeInLeverage(df);
        score += Compute6ChangeInCurrentRatio(df);
        score += Compute7ChangeInNumberOfShares(df);
        score += Compute8IncreaseGrossMargin(df);
        score += Compute9AssetTurnoverRatio(df);
        return score;
    }

    #endregion Public Methods

    #region Private Methods

    //1. Profitability
    private static int Compute1ReturnOnAssets(DerivedFinancials df)
    {
        if (df.TotalAssets + df.PyTotalAssets == 0)
        {
            return 0;
        }
        return df.CyNetIncome / ((df.TotalAssets + df.PyTotalAssets) / 2) > 0 ? 1 : 0;
    }

    //2. Operating Cash Flow
    private static int Compute2OperatingCashFlow(DerivedFinancials df)
    {
        return df.OperatingCashFlow > 0 ? 1 : 0;
    }

    //3. Change in Return of Assets
    private static int Compute3IsROABetter(DerivedFinancials df)
    {
        //divide by zero check.
        if (df.TotalAssets + df.PyTotalAssets == 0
            || df.PyTotalAssets + df.PyPyTotalAssets == 0)
        {
            return 0;
        }
        return df.CyNetIncome / ((df.TotalAssets + df.PyTotalAssets) / 2) > df.PyNetIncome / (df.PyTotalAssets + df.PyPyTotalAssets) ? 1 : 0;
    }

    //4. Accruals
    private static int Compute4Accruals(DerivedFinancials df)
    {
        //divide by zero check.
        if (df.TotalAssets == 0
            || df.TotalAssets + df.PyTotalAssets == 0)
        {
            return 0;
        }

        return df.OperatingCashFlow / df.TotalAssets > df.CyNetIncome / ((df.TotalAssets + df.PyTotalAssets) / 2) ? 1 : 0;
    }

    //5. Leverage, Liquidity and Source of Funds
    private static int Compute5ChangeInLeverage(DerivedFinancials df)
    {
        if (df.TotalAssets == 0 || df.PyTotalAssets == 0)
        {
            return 1;
        }
        return df.LongTermDebt / df.TotalAssets <= df.PyLongTermDebt / df.PyTotalAssets ? 1 : 0;
    }

    //6. Leverage, Liquidity and Source of Funds
    private static int Compute6ChangeInCurrentRatio(DerivedFinancials df)
    {
        if (df.CurrentLiabilities == 0 || df.PyCurrentLiabilities == 0)
        {
            return 0;
        }
        return df.CurrentAssets / df.CurrentLiabilities > df.PyCurrentAssets / df.PyCurrentLiabilities ? 1 : 0;
    }

    //7. Leverage, Liquidity and Source of Funds
    private static int Compute7ChangeInNumberOfShares(DerivedFinancials df)
    {
        return df.WaSharesOutstanding <= df.PyWaSharesOutstanding ? 1 : 0;
    }

    //8. Change in Gross Margin
    private static int Compute8IncreaseGrossMargin(DerivedFinancials df)
    {
        if (df.CyRevenue == 0 || df.PyRevenue == 0)
        {
            return 0;
        }
        return df.CyGrossProfit / df.CyRevenue > df.PyGrossProfit / df.PyRevenue ? 1 : 0;
    }

    //9. Change in Asset Turnover ratio
    private static int Compute9AssetTurnoverRatio(DerivedFinancials df)
    {
        if (df.TotalAssets + df.PyTotalAssets == 0
            || df.PyTotalAssets + df.PyPyTotalAssets == 0)
        {
            return 0;
        }
        return 
            df.CyRevenue / ((df.TotalAssets + df.PyTotalAssets) / 2)
            >
            df.PyRevenue / ((df.PyTotalAssets + df.PyPyTotalAssets) / 2)
             ? 1 : 0;
    }

    #endregion Private Methods
}