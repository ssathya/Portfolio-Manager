using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class MovAvgCharting
{
    [Parameter]
    public IEnumerable<SmaResult>? firstChartValues { get; set; }

    [Parameter]
    public IEnumerable<SmaResult>? secondChartValues { get; set; }

    [Parameter]
    public List<Quote>? Quotes { get; set; }

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

        if (firstChartValues == null || secondChartValues == null || Quotes == null)
        {
            return;
        }
        List<(string Date, double smaValue)> fstChtValues, secondChtValues;
        List<(string Date, double closingPrice)> quoteValues;
        ModifyInputValues(out fstChtValues, out secondChtValues, out quoteValues);
        firstChart ??= new();
        await firstChart.Clear();
        List<double> chartData = fstChtValues.Select(x => x.smaValue).ToList();
        List<string> selectedDates = fstChtValues.Select(x => x.Date).ToList();
        LineChartDataset<double> firstChartDataset = new LineChartDataset<double>
        {
            Label = "First Period",
            Data = fstChtValues.Select(x => x.smaValue).ToList(),
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await firstChart.AddDataSet(new LineChartDataset<double>
        {
            Label = "Second Period",
            Data = secondChtValues.Select(x => x.smaValue).ToList(),
            BackgroundColor = backgroundColors[1],
            BorderColor = borderColors,
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        });
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
        await firstChart.AddLabelsDatasetsAndUpdate(fstChtValues.Select(x => x.Date).ToList(), firstChartDataset);

        return;
    }

    private void ModifyInputValues(out List<(string Date, double smaValue)> fstChtValues, out List<(string Date, double smaValue)> secondChtValues, out List<(string Date, double closingPrice)> quoteValues)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        int skipValues = firstChartValues.Count(x => x.Sma == null || x.Sma == 0);
        int secondSkipValues = secondChartValues.Count(x => x.Sma == null || x.Sma == 0);
#pragma warning restore CS8604 // Possible null reference argument.
        skipValues = skipValues > secondSkipValues ? skipValues : secondSkipValues;
        skipValues++;

        fstChtValues = firstChartValues.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), x.Sma ?? 0))
            .ToList();
        secondChtValues = secondChartValues.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
           .Select(x => (x.Date.ToString("MMM-dd"), x.Sma ?? 0))
           .ToList();
#pragma warning disable CS8604 // Possible null reference argument.
        quoteValues = Quotes.Skip(skipValues)
            .Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), (double)x.Close))
            .ToList();
#pragma warning restore CS8604 // Possible null reference argument.
    }
}