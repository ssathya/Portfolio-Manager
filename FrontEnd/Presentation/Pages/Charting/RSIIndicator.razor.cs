using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data.Charts;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class RSIIndicator
{
    protected bool disableSubmitButton = true;
    protected Dictionary<string, string> DisplayValues = new();
    protected int lookBackPeriod = 14;
    protected int overBought = 70;
    protected int overSold = 30;
    protected List<Quote> quotes = new();
    protected IEnumerable<RsiResult>? RSIResult;
    protected IndexComponent? selectedIndexComponent;
    protected bool showChart = false;

    [Inject]
    protected RSIService? RSIServiceInjected { get; set; }

    protected Task DataEntryChange(object value)
    {
        UpdateDisplaySubmitButton();
        showChart = false;
        return Task.CompletedTask;
    }

    protected async Task GenerateCharts()
    {
        if (RSIServiceInjected == null || selectedIndexComponent == null)
        {
            return;
        }
        quotes = await RSIServiceInjected.ExecAsync(selectedIndexComponent.Ticker);
        RSIResult = await RSIServiceInjected.ExecAsync(selectedIndexComponent.Ticker, lookBackPeriod);
        DisplayValues.Clear();
        DisplayValues["Ticker"] = selectedIndexComponent.Ticker;
        DisplayValues["CompanyName"] = selectedIndexComponent.CompanyName;
        showChart = true;
        disableSubmitButton = true;
    }

    protected Task OnSelectedRowChangedAsync(IndexComponent indexComponent)
    {
        selectedIndexComponent = indexComponent;
        UpdateDisplaySubmitButton();
        showChart = false;
        return Task.CompletedTask;
    }

    private void UpdateDisplaySubmitButton()
    {
        if (selectedIndexComponent != null &&
             overSold > 2 && overSold < 50 &&
             overBought > 50 && overBought < 99 &&
             overBought > overSold)
        {
            disableSubmitButton = false;
        }
        else
        {
            disableSubmitButton = true;
        }
    }
}