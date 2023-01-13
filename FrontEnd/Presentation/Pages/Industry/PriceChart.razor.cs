using ApplicationModels.Quotes;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace Presentation.Pages.Industry;

public partial class PriceChart
{
    [Inject]
    public HttpClient? Client { get; set; }

    [Inject]
    protected ILogger<PriceChart>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };

    protected LineChart<decimal> lineChart = new();
    private List<decimal> closingPrices = new();
    protected YPrice? yPrice { get; set; }
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };

    protected override async Task OnParametersSetAsync()
    {
        if (Client == null)
        {
            return;
        }
        try
        {
            yPrice = (await Client.GetFromJsonAsync<YPrice>($"api/Price/{SelectedTicker}")) ?? new();
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
        if (yPrice.CompressedQuotes.Any())
        {
            await SetLineChart(yPrice);
        }
    }

    private async Task SetLineChart(YPrice yPrice)
    {
        int daysToSkip = 10;
        List<(string Dates, decimal ClosingPrice)> aaa = yPrice.CompressedQuotes.Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.Date.ToString("MMM-dd"), x.ClosingPrice))
            .ToList();
        closingPrices = aaa.Select(x => x.ClosingPrice).ToList();
        List<string> selectedDates = aaa.Select(x => x.Dates).ToList();

        await lineChart.Clear();
        LineChartDataset<decimal> lineChartDataset = new LineChartDataset<decimal>
        {
            Label = $"One year {SelectedTicker} pricing",
            Data = closingPrices,
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await lineChart.AddLabelsDatasetsAndUpdate(selectedDates, lineChartDataset);
    }
}