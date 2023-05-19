using ApplicationModels.Compute;
using ApplicationModels.ViewModel;
using Microsoft.AspNetCore.Components;
using Presentation.Data;
using Presentation.Data.Charts;

namespace Presentation.Pages.Charting;

public partial class LinReg
{
    protected List<SecurityDetails>? securityDetails;

    protected decimal selectedMom = -300M, selectedDV;

    protected int selectedPScore, selectedSimFinScore;

    [Inject]
    protected ComputeLinReg? computeLinReg { get; set; }

    [Inject]
    protected MomMfDolAvgsService? momMfDolAvg { get; set; }

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
        if (computeLinReg == null || momMfDolAvg == null)
        {
            securityDetails = new();
            return;
        }
        List<MomMfDolAvg>? momMfDolAvgs = await momMfDolAvg.ExecAsync();
        securityDetails = await computeLinReg.ExecAsync();
        if (momMfDolAvgs != null)
        {
            foreach (var sd in securityDetails)
            {
                MomMfDolAvg? mDA = momMfDolAvgs.FirstOrDefault(x => x.Ticker == sd.Ticker);
                if (mDA != null)
                {
                    sd.DollarVolume = mDA.DollarVolume;
                }
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
}