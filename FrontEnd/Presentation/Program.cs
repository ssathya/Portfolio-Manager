using AppCommon.Services;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Presentation.Data;
using PsqlAccess.SecListMaintain;
using PsqlAccess;

const string ApplicationName = "Presentation";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
IConfiguration? Configuration = ServiceHandler.GetConfiguration(ApplicationName);
ServiceHandler.SetupDatabaseConnection(builder.Services, Configuration, forceConnect: true);
ServiceHandler.SetupLogger(builder.Services, Configuration, ApplicationName);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(Configuration["ApiURL"] ?? "https://localhost:7158/") });
SetupDI(builder.Services);

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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
}