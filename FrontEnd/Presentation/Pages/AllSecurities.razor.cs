using ApplicationModels.Indexes;
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
        List<IndexComponent>? dataInDb = await Client.GetFromJsonAsync<List<IndexComponent>>("Securities");
        if (dataInDb == null)
        {
            return;
        }
        foreach (var ic in dataInDb)
        {
            indexComponents.Add(ic);
        }
    }

    //protected override void OnInitialized()
    //{
    //    int i = 0;
    //    indexComponents.Add(new()
    //    {
    //        Id = i++,
    //        CompanyName = "3M",
    //        Sector = "Industrials",
    //        SubSector = "Industrial Conglomerates",
    //        Ticker = "MMM",
    //        ListedInSnP = true,
    //        ListedInDow = false,
    //        ListedInNasdaq = false
    //    });
    //    indexComponents.Add(new()
    //    {
    //        Id = i++,
    //        CompanyName = "A.O.Smith",
    //        Sector = "Industrials",
    //        SubSector = "Building Products",
    //        Ticker = "AOS",
    //        ListedInSnP = true,
    //        ListedInDow = false,
    //        ListedInNasdaq = false
    //    });
    //    indexComponents.Add(new()
    //    {
    //        Id = i++,
    //        CompanyName = "Microsoft",
    //        Sector = "Information Technology",
    //        SubSector = "Systems Software",
    //        Ticker = "MSFT",
    //        ListedInSnP = true,
    //        ListedInDow = true,
    //        ListedInNasdaq = true
    //    });
    //}
}