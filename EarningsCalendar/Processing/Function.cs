using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.EarningsCal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace EarningsCalendar.Processing;

public class Function
{
    private const string ApplicationName = "EarningCalendar";
    private ILogger<Function>? logger;

    internal async Task ExecuteAsync()
    {
        await DoApplicationProcessingAsync();
    }

    private async Task DoApplicationProcessingAsync()
    {
        IServiceCollection services = ServiceHandler.ConfigureServices(ApplicationName);
        AppSecificSettings(services);
        ConnectToDb(services);
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<Function>>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }

        logger.LogInformation("Starting application");
        ConsumeFinnhubCalendar? consumeFinnhubCalendar = provider.GetService<ConsumeFinnhubCalendar>();
        if (consumeFinnhubCalendar == null)
        {
            logger.LogError("Could not create object to get data from Finnhub");
            return;
        }
        FinnhubCal? finnhubCal = await consumeFinnhubCalendar.GetValuesFromVendor();
    }

    private void ConnectToDb(IServiceCollection services)
    {
        IConfiguration configuration = ServiceHandler.GetConfiguration(ApplicationName);
        string? connectionStr = configuration["ConnectionString:DefaultConnection"];
        if (!string.IsNullOrEmpty(connectionStr))
        {
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseNpgsql(connectionStr));
        }
        else
        {
            Console.WriteLine("Unable to get connection string");
        }
    }

    private void AppSecificSettings(IServiceCollection services)
    {
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ConsumeFinnhubCalendar>();
        services.AddScoped<IHandleCache, HandleCache>();
    }
}