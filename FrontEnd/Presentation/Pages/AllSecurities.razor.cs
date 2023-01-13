using ApplicationModels.Quotes;
using ApplicationModels.Views;
using Microsoft.AspNetCore.Components;
using Presentation.ViewModel;

namespace Presentation.Pages;

public partial class AllSecurities
{
    private List<SecurityDetails> indexComponents = new();
    private SecurityDetails selectedIC = new();
    protected int selectedPScore;
    protected decimal selectedDV;
    protected bool DisplaySubPages;

    [Inject]
    public HttpClient? Client { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Client == null)
        {
            return;
        }
        List<SecurityWithPScore>? dataInDb = await Client.GetFromJsonAsync<List<SecurityWithPScore>>("api/SecurityWithPScores");
        List<DollarVolume>? dollarVolumes = await Client.GetFromJsonAsync<List<DollarVolume>>("api/DollarVolume");

        dollarVolumes ??= new();
        if (dataInDb == null)
        {
            return;
        }
        foreach (var ic in dataInDb)
        {
            indexComponents.Add(ic);
            var secDetail = indexComponents.Last();
            if (secDetail == null)
            {
                continue;
            }
            var dollarVolume = dollarVolumes.Where(x => x.Ticker == secDetail.Ticker).FirstOrDefault();
            if (dollarVolume != null)
            {
                secDetail.DollarVolume = dollarVolume.Value;
            }
        }
    }

    protected bool OnPScoreFilter(object itemValue, object passedValue)
    {
        if (passedValue == null || itemValue == null)
        {
            return true;
        }
        return ((int)itemValue >= (int)passedValue);
    }

    protected bool OnDvFilter(object itemValue, object passedValue)
    {
        if (passedValue == null || itemValue == null)
        {
            return true;
        }
        return ((decimal)itemValue >= (decimal)passedValue);
    }

    protected Task OnSelectedRowChangedAsync(SecurityDetails securityDetails)
    {
        selectedIC = securityDetails;
        DisplaySubPages = true;
        return Task.CompletedTask;
    }
}