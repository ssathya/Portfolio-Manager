using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class MACDCharting
{
    private const int daysToSkip = 10;
    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(103, 145, 194, 0.8f), ChartColor.FromRgba(0, 112, 0, 0.8f), ChartColor.FromRgba(112, 0, 0, 0.8f), ChartColor.FromRgba(192, 192, 0, 0.8f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    protected Chart<double>? histogramChart1;
    protected LineChart<double>? quoteChart;

    [Parameter]
    public Dictionary<string, string>? DisplayValues { get; set; }

    [Parameter]
    public IEnumerable<MacdResult>? MACDResult { get; set; }

    [Parameter]
    public List<Quote>? Quotes { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }
        await DrawCharts();
    }

    //protected override async Task OnParametersSetAsync()
    //{
    //    await DrawCharts();
    //}

    private async Task DrawCharts()
    {
        if (MACDResult == null || Quotes == null || DisplayValues == null)
        {
            return;
        }
        List<(string Date, double value)> mACD = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Macd ?? 0))
                                             .ToList();
        List<(string Date, double value)> signal = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Signal ?? 0))
                                             .ToList();
        List<(string Date, double value)> histogram = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Histogram ?? 0))
                                             .ToList();
        List<(string Date, double value)> quoteValues = Quotes.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), (double)x.Close))
                                             .ToList();
        histogramChart1 ??= new();
        await histogramChart1.Clear();
        BarChartDataset<double> histogramCharting = new BarChartDataset<double>
        {
            Label = "+ve Histogram",
            Data = histogram.Select(x => x.value > 0 ? x.value : 0).ToList(),
            BackgroundColor = backgroundColors[1],
            BorderColor = borderColors,
            BorderWidth = 1
        };
        await histogramChart1.AddDataSet(new ChartDataset<double>
        {
            Label = "-ve Histogram",
            Data = histogram.Select(x => x.value <= 0 ? x.value : 0).ToList(),
            BackgroundColor = backgroundColors[2],
            BorderColor = borderColors,
            BorderWidth = 1
        });
        await histogramChart1.AddDataSet(new LineChartDataset<double>
        {
            Label = "MACD",
            Data = mACD.Select(x => x.value).ToList(),
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        await histogramChart1.AddDataSet(new LineChartDataset<double>
        {
            Label = "Signal",
            Data = signal.Select(x => x.value).ToList(),
            BackgroundColor = backgroundColors[3],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        await histogramChart1.AddLabelsDatasetsAndUpdate(histogram.Select(x => x.Date).ToList(), histogramCharting);

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
}