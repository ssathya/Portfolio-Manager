using ApplicationModels.Compute;
using ApplicationModels.ViewModel;
using ApplicationModels.Views;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;

namespace Presentation.Pages;

public partial class AllSecurities
{
    protected bool DisplaySubPages;
    protected decimal selectedDV, selectedMom, selectedMF;
    protected int selectedPScore;

    private List<SecurityDetails> indexComponents = new();
    private SecurityDetails selectedIC = new();

    [Inject]
    protected HttpClient? Client { get; set; }

    [Inject]
    protected IBlazorDownloadFileService? BlazorDownloadFileService { get; set; }

    protected async Task ExcelDownload()
    {
        if (Client == null || BlazorDownloadFileService == null)
        {
            return;
        }
        var fileName = $"MyReport{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.xlsx";
        var response = await Client.GetAsync("api/Excel");
        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"Data size if {fileBytes.Length}");
            DownloadFileResult reslut = await BlazorDownloadFileService.DownloadFile(fileName, fileBytes, "application/octet-stream");
            Console.WriteLine(reslut.Succeeded ? "Went well" : reslut.ErrorMessage);
        }
        else
        {
            Console.WriteLine("Failed getting data from service");
        }
    }

    protected bool OnDvFilter(object itemValue, object passedValue)
    {
        if (passedValue == null || itemValue == null)
        {
            return true;
        }
        return ((decimal)itemValue >= (decimal)passedValue);
    }

    protected override async Task OnInitializedAsync()
    {
        if (Client == null)
        {
            return;
        }
        List<SecurityWithPScore>? dataInDb = await Client.GetFromJsonAsync<List<SecurityWithPScore>>("api/SecurityWithPScores");
        List<MomMfDolAvg>? momMfDolAvgs = await Client.GetFromJsonAsync<List<MomMfDolAvg>>("api/MomMfDolAvgs");
        momMfDolAvgs ??= new();

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
            MomMfDolAvg? mfDolAvgs = momMfDolAvgs.FirstOrDefault(x => x.Ticker == secDetail.Ticker);
            if (mfDolAvgs != null)
            {
                secDetail.DollarVolume = mfDolAvgs.DollarVolume;
                secDetail.Momentum = mfDolAvgs.Momentum;
                secDetail.MoneyFlow = mfDolAvgs.MoneyFlow;
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

    protected Task OnSelectedRowChangedAsync(SecurityDetails securityDetails)
    {
        selectedIC = securityDetails;
        DisplaySubPages = true;
        return Task.CompletedTask;
    }
}