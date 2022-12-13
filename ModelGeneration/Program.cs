// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelGeneration;
using PsqlAccess;
using Serilog;
using System.Reflection;

IConfiguration configuration = BuildConfig();

var LoopTimes = configuration["LoopTimes"];

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
Log.Logger.Information("Application Starting!");
var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(x => x.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false))
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<GreetingService, GreetingService>();
        services.AddDbContext<AppDbContext>(
            o => o.UseNpgsql(configuration["ConnectionString:DefaultConnection"]));
    })
    .UseSerilog()
    .Build();

var svc = ActivatorUtilities.CreateInstance<GreetingService>(host.Services);
svc.Run();
Log.Logger.Information("This application does nothing; used for model creation");

static IConfigurationRoot BuildConfig()
{
    var builder = new ConfigurationBuilder();
    IConfigurationRoot configuration = builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false)
        .Build();
    var LoopTimes = configuration["LoopTimes"];
    return configuration;
}