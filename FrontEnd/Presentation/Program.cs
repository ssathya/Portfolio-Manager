using AppCommon.Services;
using BlazorDownloadFile;
using Blazored.SessionStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.JSInterop;
using OfficeOpenXml;
using Presentation.Data;
using PsqlAccess;
using PsqlAccess.SecListMaintain;
using System.Globalization;

const string ApplicationName = "Presentation";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(40);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
});

builder.Services.AddServerSideBlazor();

IConfiguration? Configuration = ServiceHandler.GetConfiguration(ApplicationName);
ServiceHandler.SetupDatabaseConnection(builder.Services, Configuration, forceConnect: true);
ServiceHandler.SetupLogger(builder.Services, Configuration, ApplicationName);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(Configuration["ApiURL"] ?? "https://localhost:7158/") });
SetupDI(builder.Services);
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);
//PV-127
builder.Services.AddBlazoredSessionStorage();
//PV-127 end

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
app.UseSession();
app.MapFallbackToPage("/_Host");

app.Run();

static void SetupDI(IServiceCollection services)
{
    services.AddScoped<ExcelServiceAllSecurities>();
    services.AddScoped<ExcelServiceFinancial>();
    services.AddScoped<ScoreDetailService>();
    services.AddScoped<IndexComponentListService>();
    services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddSingleton<BalanceSheetService>();
    services.AddSingleton<CashFlowService>();
    services.AddSingleton<ComputesService>();
    services.AddSingleton<IncomeStatementService>();
    services.AddSingleton<MomMfDolAvgsService>();
    services.AddSingleton<OverviewService>();
    services.AddScoped<PriceService>();
    services.AddSingleton<SecurityWithPScoresService>();
}