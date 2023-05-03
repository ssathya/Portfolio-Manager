using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data.Charts;
using Skender.Stock.Indicators;

namespace Presentation.Pages.Charting;

public partial class Macd
{
    protected bool disableSubmitButton = true;
    protected bool showChart = false;
    protected IndexComponent? selectedIndexComponent;
    protected int? fastPeriod, slowPeriod, signalPeriod;
    protected List<Quote> quotes = new();
    protected IEnumerable<MacdResult>? MDACresult;
    protected Dictionary<string, string> DisplayValues = new();

    [Inject]
    protected MacdService? MACDService { get; set; }

    protected Task DataEntryChange(object value)
    {
        UpdateDisplaySubmitButton();
        showChart = false;
        return Task.CompletedTask;
    }

    protected async Task GenerateCharts()
    {
        if (MACDService == null || selectedIndexComponent == null)
        {
            return;
        }
        quotes = await MACDService.ExecAsync(selectedIndexComponent.Ticker);
        MDACresult = await MACDService.ExecAsyncMacd(selectedIndexComponent.Ticker,
            fastPeriod ?? 12,
            slowPeriod ?? 26,
            signalPeriod ?? 9);
        showChart = true;
    }

    protected override void OnInitialized()
    {
        fastPeriod = 12; slowPeriod = 26; signalPeriod = 9;
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
        if (fastPeriod != null && slowPeriod != null && signalPeriod != null
            && selectedIndexComponent != null
            && fastPeriod < slowPeriod && signalPeriod < fastPeriod)
        {
            disableSubmitButton = false;
        }
        else
        {
            disableSubmitButton = true;
        }
        return;
    }
}