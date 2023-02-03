using AppCommon.Services;
using BlazorDownloadFile;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.HttpOverrides;
using OfficeOpenXml;
using Presentation.Data;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

const string ApplicationName = "Presentation";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
IConfiguration? Configuration = ServiceHandler.GetConfiguration(ApplicationName);
ServiceHandler.SetupDatabaseConnection(builder.Services, Configuration, forceConnect: true);
ServiceHandler.SetupLogger(builder.Services, Configuration, ApplicationName);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(Configuration["ApiURL"] ?? "https://localhost:7158/") });
SetupDI(builder.Services);
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

void SetupDI(IServiceCollection services)
{
    services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddSingleton<SecurityWithPScoresService>();
    services.AddSingleton<MomMfDolAvgsService>();
    services.AddSingleton<ExcelService>();
    services.AddSingleton<BalanceSheetService>();
    services.AddSingleton<CashFlowService>();
    services.AddSingleton<IncomeStatementService>();
}