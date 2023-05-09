using ApplicationModels.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Pages.Charting;

public partial class RSIIndicator
{
    protected bool disableSubmitButton = true;
    protected bool showChart = false;
    protected IndexComponent? selectedIndexComponent;
    protected int overSold = 30;
    protected int overBought = 70;
    protected int lookBackPeriod = 14;

    protected Task OnSelectedRowChangedAsync(IndexComponent indexComponent)
    {
        selectedIndexComponent = indexComponent;
        UpdateDisplaySubmitButton();
        showChart = false;
        return Task.CompletedTask;
    }

    protected Task DataEntryChange(object value)
    {
        UpdateDisplaySubmitButton();
        showChart = false;
        return Task.CompletedTask;
    }

    private void UpdateDisplaySubmitButton()
    {
        return;
    }
}