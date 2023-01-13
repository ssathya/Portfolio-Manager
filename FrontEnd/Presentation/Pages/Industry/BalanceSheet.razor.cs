using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Components;

using ExtData = ApplicationModels.FinancialStatement.AlphaVantage;

namespace Presentation.Pages.Industry;

public partial class BalanceSheet
{
    [Inject]
    public HttpClient? Client { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    private ExtData.BalanceSheet CompanyBS = new();
    protected BSReport BalanceSheetReport { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        if (Client == null)
        {
            return;
        }
        try
        {
            CompanyBS = (await Client.GetFromJsonAsync<ExtData.BalanceSheet>($"api/BalanceSheet/{SelectedTicker}")) ?? new();
        }
        catch (Exception ex)
        {
            CompanyBS = new()
            {
                Ticker = $"Error getting data for {SelectedTicker}"
            };
            Console.WriteLine(ex.Message);
            BalanceSheetReport = new();
            return;
        }
        if (string.IsNullOrEmpty(CompanyBS.Ticker))
        {
            CompanyBS = new()
            {
                Ticker = $"Could not get data for {SelectedTicker}"
            };
            BalanceSheetReport = new();
            return;
        }
        BalanceSheetReport = CompanyBS.AnnualReports.
            OrderByDescending(x => x.FiscalDateEnding)
            .First();
        return;
    }
}