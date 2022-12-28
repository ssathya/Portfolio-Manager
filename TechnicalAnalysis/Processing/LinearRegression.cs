using MathNet.Numerics;

namespace TechnicalAnalysis.Processing;

public static class LinearRegression
{
    public static (decimal rSquared, decimal yIntercept, decimal slope) LinRegression(decimal[] xVals, decimal[] yVals)
    {
        double[] xdata = (from x in xVals
                          select (Decimal.ToDouble(x)))
                         .ToArray();
        double[] ydata = (from y in yVals
                          select (Decimal.ToDouble(y)))
                          .ToArray();
        (double intercept, double slope) = Fit.Line(xdata, ydata);
        var rSquared = GoodnessOfFit.RSquared(xdata, ydata);
        return ((decimal)rSquared, (decimal)intercept, (decimal)slope);
    }
}