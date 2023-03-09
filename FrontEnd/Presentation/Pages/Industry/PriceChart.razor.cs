using ApplicationModels.Compute;
using ApplicationModels.Quotes;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Presentation.Data;
using System.Linq;

namespace Presentation.Pages.Industry;

public partial class PriceChart
{
    [Inject]
    protected PriceService? priceService { get; set; }

    [Inject]
    protected ILogger<PriceChart>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };

    protected LineChart<decimal>? priceChart = new();
    protected LineChart<decimal>? momentumChart = new();
    private List<decimal> closingPrices = new();
    private List<decimal> momentumValues = new();
    protected YPrice? YPrice { get; set; }
    protected Compute? computes = new();
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };

    protected override async Task OnParametersSetAsync()
    {
        if (priceService == null)
        {
            return;
        }
        try
        {
            YPrice = (await priceService.ExecAsync(SelectedTicker)) ?? new();
            computes = await priceService.GetComputedValues(SelectedTicker);
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.LogError($"Unable to get pricing information for {SelectedTicker}");
                logger.LogError($"{ex.Message}", ex);
            }
            return;
        }
        if (YPrice.CompressedQuotes.Any())
        {
            await SetLineChart(YPrice, computes);
        }
    }

    private async Task SetLineChart(YPrice yPrice, Compute? computes)
    {
        int daysToSkip = 5;
        //////Work here
        if (computes == null || computes.ComputedValues.Count == 0)
        {
            return;
        }
        var compressedQuotes = yPrice.CompressedQuotes;

        var availableDates = computes.ComputedValues.Select(x => x.ReportingDate.Date);
        compressedQuotes = compressedQuotes.Where(x => availableDates.Contains(x.Date.Date))
            .ToList();

        List<(DateTime Dates, decimal ClosingPrice)> selectedReportingDates =
            compressedQuotes.Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date, x.ClosingPrice))
            .ToList();
        closingPrices = selectedReportingDates.Select(x => x.ClosingPrice).ToList();
        momentumValues.Clear();

        momentumValues = computes.ComputedValues.Where(x => (from values in selectedReportingDates
                                                             select values.Dates.Date)
                                   .ToList().Contains(x.ReportingDate.Date))
            .Select(x => x.MomentumValue)
            .ToList();
        List<string> selectedDates = selectedReportingDates.Select(x => x.Dates.ToString("MMM-dd")).ToList();
        priceChart ??= new();
        await priceChart.Clear();

        LineChartDataset<decimal> priceChartDataset = new LineChartDataset<decimal>
        {
            Label = $"{SelectedTicker}'s pricing",
            Data = closingPrices,
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        momentumChart ??= new();
        await momentumChart.Clear();
        LineChartDataset<decimal> momentumChartDataset = new LineChartDataset<decimal>
        {
            Label = "ROC",
            Data = momentumValues,
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await priceChart.AddLabelsDatasetsAndUpdate(selectedDates, priceChartDataset);
        await momentumChart.AddLabelsDatasetsAndUpdate(selectedDates, momentumChartDataset);
    }
}