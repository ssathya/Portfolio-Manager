using ApplicationModels.Compute;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components;

namespace Presentation.Pages.Industry;

public partial class MomentumMoneyFlow
{
    [Inject]
    public HttpClient? Client { get; set; }

    [Inject]
    protected ILogger<MomentumMoneyFlow>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    private LineChartOptions lineChartOptions = new() { Scales = new ChartScales { X = new ChartAxis { Type = "linear" } } };
    protected LineChart<decimal> momentumChart = new();
    protected LineChart<decimal> moneyFlowChart = new();
    private List<decimal> momentumValues = new();
    private List<decimal> moneyFlowValues = new();
    private readonly List<string> backgroundColors = new() { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    protected string selectedTab = "momentum";

    protected override async Task OnParametersSetAsync()
    {
        Compute compute = new();
        if (Client == null || string.IsNullOrEmpty(SelectedTicker))
        {
            return;
        }
        try
        {
            compute = (await Client.GetFromJsonAsync<Compute>($"api/Computes/{SelectedTicker}")) ?? new();
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.LogError($"Unable to get Momentum and money flow for {SelectedTicker}");
                logger.LogError(ex.ToString());
            }
        }
        if (compute.Ticker.Equals(SelectedTicker))
        {
            await SetLineChart(compute);
        }
    }

    private async Task SetLineChart(Compute compute)
    {
        int daysToSkip = 10;
        List<(string Dates, decimal MomentumValue, decimal MoneyFlow)> aaa = compute.ComputedValues.Where((x, i) => i % daysToSkip == 0)
            .Select(x => (x.ReportingDate.ToString("MMM-dd"), x.MomentumValue, x.MoneyFlow))
            .ToList();
        List<string> selectedDates = aaa.Select(x => x.Dates).ToList();
        momentumValues = aaa.Select(x => x.MomentumValue).ToList();
        moneyFlowValues = aaa.Select(x => x.MoneyFlow).ToList();
        await momentumChart.Clear();
        LineChartDataset<decimal> momentumDataSet = new LineChartDataset<decimal>()
        {
            Label = $"Momentum for {SelectedTicker}",
            Data = momentumValues,
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await momentumChart.AddLabelsDatasetsAndUpdate(selectedDates, momentumDataSet);
        await moneyFlowChart.Clear();
        LineChartDataset<decimal> moneyFlowDataSet = new LineChartDataset<decimal>()
        {
            Label = $"Money Flow for {SelectedTicker}",
            Data = moneyFlowValues,
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            PointRadius = 3,
            CubicInterpolationMode = "monotone"
        };
        await moneyFlowChart.AddLabelsDatasetsAndUpdate(selectedDates, moneyFlowDataSet);
    }

    protected Task OnSelectedTabChanged(string name)
    {
        selectedTab = name;
        return Task.CompletedTask;
    }
}