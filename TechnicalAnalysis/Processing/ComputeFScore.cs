using ApplicationModels.FinancialStatement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalAnalysis.Processing;

public static class ComputeFScore
{
    private const string ProcessFilingType = "10-K";

    //Rule 1 as per Wikipedia
    public static bool ReturnOnAssets(List<FinStatements> finStatements)
    {
        List<FinStatements> fs = ArrangeFinStatements(finStatements);
        if (fs != null && fs.Count >= 2)
        {
            if ((fs[0].Assets + fs[1].Assets) == 0)
            {
                return false;
            }
            return (fs[0].NetIncome / ((fs[0].Assets + fs[1].Assets) / 2)) > 0;
        }
        return false;
    }

    private static List<FinStatements> ArrangeFinStatements(List<FinStatements> finStatements)
    {
        return finStatements
                    .Where(x => x.FilingType.Equals(ProcessFilingType))
                    .OrderByDescending(x => x.PeriodEnd).ToList()
                    .ToList();
    }

    //Rule 2 - Wikipedia
    public static bool OperatingCashFlow(List<FinStatements> finStatements)
    {
        var fs = ArrangeFinStatements(finStatements);
        if (fs != null && fs.Count > 0)
        {
            return fs[0].OperatingCashFlows > 0;
        }
        return false;
    }

    //Rule 3 - Wikipedia
    public static bool ChangeInROA(List<FinStatements> finStatements)
    {
        var fs = ArrangeFinStatements(finStatements);
        if (fs != null && fs.Count > 2)
        {
            if ((fs[0].Assets + fs[1].Assets) == 0
                || (fs[2].Assets + fs[1].Assets) == 0)
                return false;
            return (fs[0].NetIncome / ((fs[0].Assets + fs[1].Assets) / 2)
                > fs[1].NetIncome / ((fs[1].Assets + fs[2].Assets) / 2));
        }
        return false;
    }

    //Rule 4 - Wikipedia
    public static bool ChangInLongTermDebt(List<FinStatements> finStatements)
    {
        var fs = ArrangeFinStatements(finStatements);
        if (fs == null || fs.Count < 2)
        {
            return false;
        }
        return (fs[0].Liabilities / fs[0].Assets) < (fs[1].Liabilities / fs[1].Assets);
    }
}