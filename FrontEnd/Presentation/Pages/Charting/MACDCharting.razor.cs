﻿using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class MACDCharting
{
    protected string CompanyName = string.Empty;
    protected Chart<double>? MACDGraph;
    protected LineChart<double>? quoteChart;
    protected string Ticker = string.Empty;

    private const int daysToSkip = 10;
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(103, 145, 194, 0.8f), ChartColor.FromRgba(0, 112, 0, 0.8f), ChartColor.FromRgba(112, 0, 0, 0.8f), ChartColor.FromRgba(192, 192, 0, 0.8f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };

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

    protected override void OnParametersSet()
    {
        if (DisplayValues != null)
        {
            CompanyName = DisplayValues["CompanyName"];
            Ticker = DisplayValues["Ticker"];
        }
    }

    private async Task BuildBarChart(List<(string Date, double value)> mACD,
        List<(string Date, double value)> signal,
        List<(string Date, double value)> histogram)
    {
        MACDGraph ??= new();
        await MACDGraph.Clear();
        BarChartDataset<double> histogramCharting = new BarChartDataset<double>
        {
            Label = "+ve Histogram",
            Data = histogram.Select(x => x.value > 0 ? x.value : 0).ToList(),
            BackgroundColor = backgroundColors[1],
            BorderColor = borderColors,
            BorderWidth = 1
        };
        await MACDGraph.AddDataSet(new ChartDataset<double>
        {
            Label = "-ve Histogram",
            Data = histogram.Select(x => x.value <= 0 ? x.value : 0).ToList(),
            BackgroundColor = backgroundColors[2],
            BorderColor = borderColors,
            BorderWidth = 1
        });
        await MACDGraph.AddDataSet(new LineChartDataset<double>
        {
            Label = "MACD",
            Data = mACD.Select(x => x.value).ToList(),
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        await MACDGraph.AddDataSet(new LineChartDataset<double>
        {
            Label = "Signal",
            Data = signal.Select(x => x.value).ToList(),
            BackgroundColor = backgroundColors[3],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 1,
            CubicInterpolationMode = "monotone"
        });
        await MACDGraph.AddLabelsDatasetsAndUpdate(histogram.Select(x => x.Date).ToList(), histogramCharting);
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

    private async Task DrawCharts()
    {
        if (MACDResult == null || Quotes == null || DisplayValues == null)
        {
            return;
        }
        (string Date, double value) lastMacd = (MACDResult.Last().Date.ToString("MMM-dd"), MACDResult.Last().Macd ?? 0);
        (string Date, double value) lastSignal = (MACDResult.Last().Date.ToString("MMM-dd"), MACDResult.Last().Signal ?? 0);
        (string Date, double value) lastHistogram = (MACDResult.Last().Date.ToString("MMM-dd"), MACDResult.Last().Histogram ?? 0);
        (string Date, double value) lastQuotes = (Quotes.Last().Date.ToString("MMM-dd"), (double)Quotes.Last().Close);
        List<(string Date, double value)> mACD = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Macd ?? 0))
                                             .ToList();
        if (mACD.Last() != lastMacd) { mACD.Add(lastMacd); }
        List<(string Date, double value)> signal = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Signal ?? 0))
                                             .ToList();
        if (signal.Last() != lastSignal) { signal.Add(lastSignal); }
        List<(string Date, double value)> histogram = MACDResult.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), x.Histogram ?? 0))
                                             .ToList();
        if (histogram.Last() != lastHistogram) { histogram.Add(lastHistogram); }
        List<(string Date, double value)> quoteValues = Quotes.Skip(daysToSkip)
                                             .Where((x, i) => i % daysToSkip == 0)
                                             .Select(x => (x.Date.ToString("MMM-dd"), (double)x.Close))
                                             .ToList();
        if (quoteValues.Last() != lastQuotes) { quoteValues.Add(lastQuotes); }
        await BuildBarChart(mACD, signal, histogram);
        await BuildPriceChart(quoteValues);
    }
}