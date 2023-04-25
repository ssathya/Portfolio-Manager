using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data;

namespace Presentation.Pages.Charting;

public partial class MovingAverages
{
    [Inject]
    protected IndexComponentListService? indexComponentListService { get; set; }

    private List<IndexComponent>? indexComponents;
    protected int? firstValue, secondValue;

    protected List<string> chartType = new()
    {
        "Simple",
        "Exponential"
    };

    protected string selectedChartType = "Select";
    protected IndexComponent? selectedIndexComponent;
    protected bool displaySubmitButton = true;

    protected override async Task OnInitializedAsync()
    {
        if (indexComponentListService == null)
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

    protected Task GenerateCharts()
    {
        return Task.CompletedTask;
    }

    private void UpdateDisplaySubmitButton()
    {
        if (selectedIndexComponent != null &&
            selectedIndexComponent.Ticker != string.Empty &&
            firstValue != 0 && secondValue != 0)
        {
            displaySubmitButton = false;
        }
        else
        {
            displaySubmitButton = true;
        }
    }
}