using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class RSICharting
{
    protected string CompanyName = string.Empty;
    protected LineChart<double>? indicatorChart;
    protected LineChart<double>? quoteChart;
    protected string Ticker = string.Empty;

    private const int daysToSkip = 8;
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(103, 145, 194, 0.8f), ChartColor.FromRgba(0, 112, 0, 0.8f), ChartColor.FromRgba(112, 0, 0, 0.8f), ChartColor.FromRgba(192, 192, 0, 0.8f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };

    [Parameter]
    public Dictionary<string, string>? DisplayValues { get; set; }

    [Parameter]
    public int? overBought { get; set; }

    [Parameter]
    public int? overSold { get; set; }

    [Parameter]
    public List<Quote>? Quotes { get; set; }

    [Parameter]
    public IEnumerable<RsiResult>? RsiResults { get; set; }

    [Parameter]
    public int? lookBackPeriod { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }
        await DrawChart();
    }

    protected override void OnParametersSet()
    {
        if (DisplayValues != null)
        {
            CompanyName = DisplayValues["CompanyName"];
            Ticker = DisplayValues["Ticker"];
        }
    }

    private async Task BuildPriceChart(List<(string Date, double value)> quoteValues)
    {
        quoteChart ??= new();
        await quoteChart.Clear();
        LineChartDataset<double> chartDataSet = new LineChartDataset<double>
        {
            Label = "Closing Price",
            Data = quoteValues.Select(x => x.value).ToList(),
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await quoteChart.AddLabelsDatasetsAndUpdate(quoteValues.Select(x => x.Date).ToList(), chartDataSet);
    }

    private async Task BuildRsiChart(List<(string Date, double value)> rsiResults)
    {
        indicatorChart ??= new();
        await indicatorChart.Clear();
        LineChartDataset<double> lineChartDataset = new LineChartDataset<double>
        {
            Label = "RSI",
            Data = rsiResults.Select(x => x.value).ToList(),
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        };

        List<double> overSoldLst = (from x in rsiResults
                                    select ((double)(overSold ?? 0)))
                          .ToList();
        await indicatorChart.AddDataSet(new LineChartDataset<double>
        {
            Label = "Over sold",
            Data = overSoldLst,
            BackgroundColor = backgroundColors[1],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        List<double> overBoughtLst = (from x in rsiResults
                                      select ((double)(overBought ?? 0)))
                            .ToList();
        await indicatorChart.AddDataSet(new LineChartDataset<double>
        {
            Label = "Over bought",
            Data = overBoughtLst,
            BackgroundColor = backgroundColors[2],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        await indicatorChart.AddLabelsDatasetsAndUpdate(rsiResults.Select(x => x.Date).ToList(), lineChartDataset);
    }

    private async Task DrawChart()
    {
        if (RsiResults == null || Quotes == null)
        {
            return;
        }
        RsiResults = RsiResults.RemoveWarmupPeriods(lookBackPeriod ?? 14);

        var dates = RsiResults.Select(x => x.Date).ToList();
        Quotes = Quotes.Where(x => dates.Contains(x.Date)).ToList();

        List<(string, double value)> rsiResults = RsiResults.Skip(daysToSkip)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), x.Rsi ?? 0))
            .ToList();
        (string, double value) lastRsi = (RsiResults.Last().Date.ToString("MMM-dd"), RsiResults.Last().Rsi ?? 0);
        if (rsiResults.Last() != lastRsi)
        {
            rsiResults.Add(lastRsi);
        }
        List<(string, double value)> quoteValues = Quotes.Skip(daysToSkip)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), (double)x.Close))
            .ToList();
        (string Date, double value) quotesLast = (Quotes.Last().Date.ToString("MMM-dd"), (double)Quotes.Last().Close);
        if (quoteValues.Last() != quotesLast)
        {
            quoteValues.Add(quotesLast);
        }
        await BuildPriceChart(quoteValues);
        await BuildRsiChart(rsiResults);
    }
}