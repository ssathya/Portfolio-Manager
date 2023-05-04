using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Components;
using Presentation.Data;

namespace Presentation.Pages.Charting;

public partial class SelectTicker
{
    [Inject]
    protected IndexComponentListService? IndexComponentListService { get; set; }

    [Parameter]
    public EventCallback<IndexComponent> SelectionChanged { get; set; }

    protected IndexComponent? selectedIndexComponent;
    private List<IndexComponent>? indexComponents;

    protected override async Task OnInitializedAsync()
    {
        if (IndexComponentListService == null)
        {
            return;
        }
        indexComponents = (await IndexComponentListService.ExecAsync())
            .OrderBy(x => x.Ticker)
            .ToList();
    }

    protected async Task OnSelectedRowChangedAsync(IndexComponent indexComponent)
    {
        selectedIndexComponent = indexComponent;
        await SelectionChanged.InvokeAsync(selectedIndexComponent);
        return;
    }
}