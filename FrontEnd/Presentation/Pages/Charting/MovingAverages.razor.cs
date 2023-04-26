using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data;
using Presentation.Data.Charts;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class MovingAverages
{
    [Inject]
    protected IndexComponentListService? indexComponentListService { get; set; }

    [Inject]
    protected MovingAverageService? movingAverage { get; set; }

    private List<IndexComponent>? indexComponents;
    protected int? firstValue, secondValue;
    protected IEnumerable<SmaResult>? firstChartValues, secondChartValues;
    protected List<Quote> quotes = new();

    protected List<string> chartType = new()
    {
        "Simple",
        "Exponential"
    };

    protected string selectedChartType = "Select";
    protected IndexComponent? selectedIndexComponent;
    protected bool displaySubmitButton = true;
    protected bool showChart = false;

    protected override async Task OnInitializedAsync()
    {
        if (indexComponentListService == null || movingAverage == null)
        {
            return;
        }
        indexComponents = await indexComponentListService.ExecAsync();
    }

    protected Task HandleDropdownItemClicked(object value)
    {
        selectedChartType = (string)value;
        UpdateDisplaySubmitButton();
        return Task.CompletedTask;
    }

    protected Task OnSelectedRowChangedAsync(IndexComponent indexComponent)
    {
        selectedIndexComponent = indexComponent;
        UpdateDisplaySubmitButton();
        return Task.CompletedTask;
    }

    protected async Task GenerateCharts()
    {
        if (movingAverage == null || selectedIndexComponent == null)
        {
            return;
        }
        firstChartValues = await movingAverage.ExecAsync(selectedIndexComponent.Ticker, firstValue ?? 20, true);
        secondChartValues = await movingAverage.ExecAsync(selectedIndexComponent.Ticker, secondValue ?? 50, true);
        quotes = await movingAverage.ExecAsync(selectedIndexComponent.Ticker);
        showChart = true;
    }

    private void UpdateDisplaySubmitButton()
    {
        if (selectedIndexComponent != null &&
            selectedIndexComponent.Ticker != string.Empty &&
            firstValue != 0 && secondValue != 0)
        {
            displaySubmitButton = false;
            showChart = false;
        }
        else
        {
            displaySubmitButton = true;
        }
    }
}