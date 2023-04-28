using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data;
using Presentation.Data.Charts;
using Presentation.Models;
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
    protected IEnumerable<ChartingValues>? firstChartValues, secondChartValues;
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
    protected Dictionary<string, string>? MsgToDisplay;

    protected override async Task OnInitializedAsync()
    {
        if (indexComponentListService == null || movingAverage == null)
        {
            return;
        }
        indexComponents = (await indexComponentListService.ExecAsync())
            .OrderBy(x => x.Ticker)
            .ToList();
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
        if (selectedChartType.Equals(chartType[0])) //simple
        {
            IEnumerable<SmaResult> x = await movingAverage.ExecAsyncSma(selectedIndexComponent.Ticker, firstValue ?? 20, true) ?? new List<SmaResult>();
            firstChartValues = from x1 in x
                               select (ChartingValues)x1;
            x = await movingAverage.ExecAsyncSma(selectedIndexComponent.Ticker, secondValue ?? 50, true) ?? new List<SmaResult>();
            secondChartValues = from x1 in x
                                select (ChartingValues)x1;
        }
        else if (selectedChartType.Equals(chartType[1])) //exponential
        {
            var x = await movingAverage.ExecAsyncEma(selectedIndexComponent.Ticker, firstValue ?? 20, true) ?? new List<EmaResult>();
            firstChartValues = from x1 in x
                               select (ChartingValues)x1;
            x = await movingAverage.ExecAsyncEma(selectedIndexComponent.Ticker, secondValue ?? 50, true) ?? new List<EmaResult>();
            secondChartValues = from x1 in x
                                select (ChartingValues)x1;
        }

        quotes = await movingAverage.ExecAsync(selectedIndexComponent.Ticker);
        MsgToDisplay ??= new();
        MsgToDisplay.Clear();

        MsgToDisplay["FirstPeriodTitle"] = $"{firstValue} days";
        MsgToDisplay["SecondPeriodTitle"] = $"{secondValue} days";
        MsgToDisplay["ChartTitle"] = $"{selectedIndexComponent.CompanyName}'s {selectedChartType}";
        showChart = true;
    }

    private void UpdateDisplaySubmitButton()
    {
        if (selectedIndexComponent != null &&
            selectedIndexComponent.Ticker != string.Empty &&
            firstValue != null && secondValue != null &&
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