using ApplicationModels.Indexes;
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
        if (dataInDb == null)
        {
            return;
        }
        foreach (var ic in dataInDb)
        {
            indexComponents.Add(ic);
        }
    }
}