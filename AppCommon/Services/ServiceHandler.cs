using AppCommon.CacheHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using Serilog;
using System.Text;

namespace AppCommon.Services;

public static class ServiceHandler
{
    private static IConfiguration? Configuration;

    public static IServiceCollection ConfigureServices(string applicationName)
    {
        IServiceCollection services = new ServiceCollection();
        Configuration = BuildConfiguration(applicationName);
        services.AddSingleton<IConfiguration>(_ => Configuration);

        //Logger
        SetupLogger(services, Configuration, applicationName);
        //Setup db connection
        SetupDatabaseConnection(services, Configuration);

        //for cache
        services.AddScoped<HttpClient>();
        //Setup Cache service
        CacheInitialize.Initialize(applicationName);

        return services;
    }

    public static IConfiguration GetConfiguration(string applicationName)
    {
        if (Configuration == null)
        {
            Configuration = BuildConfiguration(applicationName);
        }
        return Configuration;
    }

    private static IConfiguration BuildConfiguration(string applicationName)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json"
                , optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddSystemsManager(@"/" + applicationName + @"/", TimeSpan.FromMinutes(5))
            .AddSystemsManager("""/PortfolioManager/""");

        Configuration = builder.Build();
        return Configuration;
    }

    public static void SetupLogger(IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        StringBuilder filePath = new();
        filePath.Append(Path.GetTempPath() + "/");
        var today = DateTime.Now.ToString("yyyyMMdd");
        filePath.Append($"{applicationName}-.log");
        //string filePath = Path.GetTempPath();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.RollingFile(filePath.ToString(), retainedFileCountLimit: 4)
            .CreateLogger();
        services.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddSerilog(Log.Logger);
        });
        Log.Logger.Information("Application starting...");
    }

    public static void SetupDatabaseConnection(IServiceCollection services, IConfiguration configuration, bool forceConnect = false)
    {
        string? connectionString = configuration["ConnectionString:DefaultConnection"];
        if (connectionString == null)
        {
            Console.WriteLine("Unable to get connection string");
            return;
        }
        if (forceConnect && !string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
}