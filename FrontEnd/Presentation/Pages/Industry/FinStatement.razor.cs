using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Presentation.Data;

namespace Presentation.Pages.Industry;

public partial class FinStatement
{
    [Inject]
    protected ILogger<FinStatement>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    [Inject]
    protected ExcelServiceFinancial? excelServiceFinancial { get; set; }

    [Inject]
    protected IBlazorDownloadFileService? blazorDownloadFileService { get; set; }

    protected async Task ExcelDownload()
    {
        if (excelServiceFinancial == null || blazorDownloadFileService == null
            || string.IsNullOrEmpty(SelectedTicker)) { return; }
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        string fileName = $"{SelectedTicker}-Financial-{todayDate}.xlsx";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        FileContentResult? response = await excelServiceFinancial.ExecAsync(SelectedTicker);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (response == null) { return; }
        var finalBytes = response.FileContents.ToArray();
        DownloadFileResult? downloadFileResult = await blazorDownloadFileService.DownloadFile(
            fileName,
            finalBytes,
            "application/octet-stream");
        if (!downloadFileResult.Succeeded)
        {
            if (logger != null)
            {
                logger.LogInformation("Excel download failed");
            }
            else
            {
                await Console.Out.WriteLineAsync("Excel download failed");
            }
        }
    }
}