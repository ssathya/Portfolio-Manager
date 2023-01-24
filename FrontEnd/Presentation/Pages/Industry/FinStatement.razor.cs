using Microsoft.AspNetCore.Components;
using ExtData = ApplicationModels.FinancialStatement.AlphaVantage;

namespace Presentation.Pages.Industry;

public partial class FinStatement
{
    [Inject]
    public HttpClient? Client { get; set; }

    [Inject]
    protected ILogger<FinStatement>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    protected ExtData.BSReport BalanceSheetReport { get; set; } = new();
    protected ExtData.CashFlowReport CashFlowReport { get; set; } = new();
    protected ExtData.IncomeReport IncomeReport { get; set; } = new();
    protected string selectedTab = "balanceSheet";

    protected override async Task OnParametersSetAsync()
    {
        selectedTab = "balanceSheet";
        if (Client == null || logger == null)
        {
            return;
        }
        ExtData.BalanceSheet? companyBS;
        ExtData.CashFlow? companyCF;
        ExtData.IncomeStatement? companyIS;
        try
        {
            companyBS = await Client.GetFromJsonAsync<ExtData.BalanceSheet>($"api/BalanceSheet/{SelectedTicker}");
            companyCF = await Client.GetFromJsonAsync<ExtData.CashFlow>($"api/CashFlow/{SelectedTicker}");
            companyIS = await Client.GetFromJsonAsync<ExtData.IncomeStatement>($"api/IncomeStatement/{SelectedTicker}");
            BalanceSheetReport = new();
            CashFlowReport = new();
            IncomeReport = new();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting financial information for ticker {SelectedTicker}");
            logger.LogError($"{ex.Message}", ex);
            return;
        }
        if (companyBS == null || companyCF == null || companyIS == null)
        { return; }
        BalanceSheetReport = companyBS.AnnualReports
            .OrderByDescending(x => x.FiscalDateEnding)
            .First();
        CashFlowReport = companyCF.AnnualReports
            .OrderByDescending(x => x.FiscalDateEnding).First();
        IncomeReport = companyIS.AnnualReports
            .OrderByDescending(x => x.FiscalDateEnding).First();
    }

    protected Task OnSelectedTabChanged(string name)
    {
        selectedTab = name;
        return Task.CompletedTask;
    }
}