using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Components;
using Presentation.Data;

using ExtData = ApplicationModels.FinancialStatement.AlphaVantage;

namespace Presentation.Pages.Industry;

public partial class FinStatement
{
    //[Inject]
    //public HttpClient? Client { get; set; }
    [Inject]
    protected BalanceSheetService? balanceSheet { get; set; }

    [Inject]
    protected CashFlowService? cashFlow { get; set; }

    [Inject]
    protected IncomeStatementService? incomeStatement { get; set; }

    [Inject]
    protected ILogger<FinStatement>? logger { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    protected BSReport BalanceSheetReport { get; set; } = new();
    protected CashFlowReport CashFlowReport { get; set; } = new();
    protected IncomeReport IncomeReport { get; set; } = new();
    protected string selectedTab = "balanceSheet";

    protected override async Task OnParametersSetAsync()
    {
        selectedTab = "balanceSheet";
        if (balanceSheet == null || cashFlow == null || incomeStatement == null || logger == null)
        {
            return;
        }
        ExtData.BalanceSheet? companyBS;
        CashFlow? companyCF;
        IncomeStatement? companyIS;
        try
        {
            companyBS = await balanceSheet.ExecAsync(SelectedTicker);
            companyCF = await cashFlow.ExecAsync(SelectedTicker);
            companyIS = await incomeStatement.ExecAsync(SelectedTicker);
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