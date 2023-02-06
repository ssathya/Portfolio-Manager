using ApplicationModels.Compute;
using Microsoft.AspNetCore.Components;
using Presentation.Data;

namespace Presentation.Pages.Industry;

public partial class ScoreDetailList
{
    protected ScoreDetail ScoreDetail { get; set; } = new ScoreDetail();

    [Inject]
    protected ScoreDetailService? ScoreDetailService { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (ScoreDetailService == null) return;
        try
        {
            ScoreDetail = await ScoreDetailService.ExecAsync(SelectedTicker);
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to get P-Score details for {SelectedTicker}");
        }
    }
}