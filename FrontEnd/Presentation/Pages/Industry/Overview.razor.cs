using Microsoft.AspNetCore.Components;
using Presentation.ViewModel;
using ExtData = ApplicationModels.FinancialStatement.AlphaVantage;

namespace Presentation.Pages.Industry;

public partial class Overview
{
    #region Private Fields

    private ExtData.Overview CompanyProfile = new();

    #endregion Private Fields

    #region Public Properties

    [Inject]
    public HttpClient? Client { get; set; }

    [Parameter]
    public string SelectedTicker { get; set; } = string.Empty;

    [Parameter]
    public SecurityDetails securityDetails { get; set; } = new();

    #endregion Public Properties

    #region Protected Methods

    //protected override async Task OnInitializedAsync()
    //{
    //    await GetExternalValues();
    //}

    protected override async Task OnParametersSetAsync()
    {
        await GetExternalValues();
    }

    #endregion Protected Methods

    #region Private Methods

    private async Task GetExternalValues()
    {
        if (Client == null)
        {
            return;
        }
        ExtData.Overview? overview;
        try
        {
            overview = await Client.GetFromJsonAsync<ExtData.Overview>($"api/Overview/{SelectedTicker}");
        }
        catch (Exception)
        {
            CompanyProfile = new()
            {
                Name = $"Unhanded error getting values for {SelectedTicker}"
            };
            return;
        }
        if (overview == null)
        {
            CompanyProfile = new()
            {
                Name = $"The system is not updated with information about {SelectedTicker}",
                Ticker = SelectedTicker
            };
            return;
        }
        CompanyProfile = overview;
        return;
    }

    #endregion Private Methods
}