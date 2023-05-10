using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Presentation.Models;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class MovAvgCharting
{
    [Parameter]
    public IEnumerable<ChartingValues>? firstChartValues { get; set; }

    [Parameter]
    public IEnumerable<ChartingValues>? secondChartValues { get; set; }

    [Parameter]
    public List<Quote>? Quotes { get; set; }

    [Parameter]
    public Dictionary<string, string>? DisplayValues { get; set; }

    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    protected LineChart<double>? firstChart;

    private const int daysToSkip = 10;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (firstChartValues == null || secondChartValues == null || Quotes == null || DisplayValues == null)
        {
            return;
        }
        ModifyInputValues(out List<(string Date, double smaValue)> fstChtValues,
            out List<(string Date, double smaValue)> secondChtValues,
            out List<(string Date, double closingPrice)> quoteValues);
        firstChart ??= new();
        await firstChart.Clear();
        List<double> chartData = fstChtValues.Select(x => x.smaValue).ToList();
        List<string> selectedDates = fstChtValues.Select(x => x.Date).ToList();
        LineChartDataset<double> firstChartDataset = new LineChartDataset<double>
        {
            Label = DisplayValues["FirstPeriodTitle"],
            Data = fstChtValues.Select(x => x.smaValue).ToList(),
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await firstChart.AddDataSet(new LineChartDataset<double>
        {
            Label = "Closing Price",
            Data = quoteValues.Select(x => x.closingPrice).ToList(),
            BackgroundColor = backgroundColors[2],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        });
        await firstChart.AddDataSet(new LineChartDataset<double>
        {
            Label = DisplayValues["SecondPeriodTitle"],
            Data = secondChtValues.Select(x => x.smaValue).ToList(),
            BackgroundColor = backgroundColors[1],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        });

        await firstChart.AddLabelsDatasetsAndUpdate(fstChtValues.Select(x => x.Date).ToList(), firstChartDataset);

        return;
    }

    private void ModifyInputValues(out List<(string Date, double smaValue)> fstChtValues,
        out List<(string Date, double smaValue)> secondChtValues,
        out List<(string Date, double closingPrice)> quoteValues)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        int skipValues = firstChartValues.Count(x => x.Value == null || x.Value == 0);
        int secondSkipValues = secondChartValues.Count(x => x.Value == null || x.Value == 0);
        (string Date, double value) fstChtLast = (firstChartValues.Last().Date.ToString("MMM-dd"), firstChartValues.Last().Value ?? 0);
        (string Date, double value) secondChtLast = (secondChartValues.Last().Date.ToString("MMM-dd"), secondChartValues.Last().Value ?? 0);
        (string Date, double value) quotesLast = (Quotes.Last().Date.ToString("MMM-dd"), (double)Quotes.Last().Close);
#pragma warning restore CS8604 // Possible null reference argument.
        skipValues = skipValues > secondSkipValues ? skipValues : secondSkipValues;
        skipValues++;

        fstChtValues = firstChartValues.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), x.Value ?? 0))
            .ToList();
        if (fstChtValues.Last() != fstChtLast) { fstChtValues.Add(fstChtLast); }
        secondChtValues = secondChartValues.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
           .Select(x => (x.Date.ToString("MMM-dd"), x.Value ?? 0))
           .ToList();
        if (secondChtValues.Last() != secondChtLast) { secondChtValues.Add(secondChtLast); }
#pragma warning disable CS8604 // Possible null reference argument.
        quoteValues = Quotes.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), (double)x.Close))
            .ToList();
        if (quoteValues.Last() != quotesLast) { quoteValues.Add(quotesLast); }
#pragma warning restore CS8604 // Possible null reference argument.
    }
}