using ApplicationModels.Compute;
using ApplicationModels.ViewModel;
using ApplicationModels.Views;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Presentation.Data;

namespace Presentation.Pages;

public partial class AllSecurities
{
    #region Protected Fields

    protected bool DisplaySubPages;
    protected decimal selectedDV, selectedMom, selectedMF;
    protected int selectedPScore;

    #endregion Protected Fields

    #region Private Fields

    private List<SecurityDetails> indexComponents = new();
    private SecurityDetails selectedIC = new();

    #endregion Private Fields

    #region Protected Properties

    [Inject]
    protected IBlazorDownloadFileService? BlazorDownloadFileService { get; set; }

    [Inject]
    protected ExcelServiceAllSecurities? excelService { get; set; }

    [Inject]
    protected MomMfDolAvgsService? momMfDolAvgs { get; set; }

    [Inject]
    protected SecurityWithPScoresService? securityWithPScores { get; set; }

    #endregion Protected Properties

    #region Protected Methods

    protected async Task ExcelDownload()
    {
        if (excelService == null || BlazorDownloadFileService == null)
        {
            return;
        }
        var fileName = $"MyReport{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.xlsx";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var response = await excelService.ExecAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (response == null)
        {
            return;
        }
        var fineBytes = response.FileContents.ToArray();
        DownloadFileResult result = await BlazorDownloadFileService.DownloadFile(fileName, fineBytes, "application/octet-stream");
        if (!result.Succeeded)
        {
            Console.WriteLine("Excel download failed");
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
        if (this.momMfDolAvgs == null || securityWithPScores == null)
        {
            return;
        }
        List<SecurityWithPScore>? dataInDb = await securityWithPScores.ExecAsync();
        List<MomMfDolAvg>? momMfDolAvgs = await this.momMfDolAvgs.ExecAsync();
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

    #endregion Protected Methods
}