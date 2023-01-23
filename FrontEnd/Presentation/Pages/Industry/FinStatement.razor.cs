using Microsoft.AspNetCore.Components;
using ExtData = ApplicationModels.FinancialStatement.AlphaVantage;

namespace Presentation.Pages.Industry;

public partial class FinStatement
{
    [Inject]
    public HttpClient? Client { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    private ExtData.BalanceSheet CompanyBS = new();
    protected ExtData.BSReport BalanceSheetReport { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        return base.OnParametersSetAsync();
    }
}