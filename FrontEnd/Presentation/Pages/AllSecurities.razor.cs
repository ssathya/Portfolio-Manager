using ApplicationModels.Quotes;
using ApplicationModels.Views;
using Microsoft.AspNetCore.Components;
using Presentation.ViewModel;

namespace Presentation.Pages;

public partial class AllSecurities
{
    private List<SecurityDetails> indexComponents = new();
    private SecurityDetails selectedIC = new();

    [Inject]
    public HttpClient? Client { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Client == null)
        {
            return;
        }
        List<SecurityWithPScore>? dataInDb = await Client.GetFromJsonAsync<List<SecurityWithPScore>>("api/SecurityWithPScores");
        //List<YPrice>? yPrices = await Client.GetFromJsonAsync<List<YPrice>>("api/Price");
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
}