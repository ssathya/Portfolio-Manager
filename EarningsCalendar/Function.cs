using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.EarningsCal;
using EarningsCalendar.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace EarningsCalendar;

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
        ConsumeFinnhubCalendar? consumeFinnhubCalendar = provider.GetService<ConsumeFinnhubCalendar>();
        EarningsCalToDb? earningsCalToDb = provider.GetService<EarningsCalToDb>();

        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }

        logger.LogInformation("Starting application");
        if (consumeFinnhubCalendar == null)
        {
            logger.LogError("Could not create object to get data from Finnhub");
            return;
        }
        FinnhubCal? finnhubCal = await consumeFinnhubCalendar.GetValuesFromVendor();
        if (earningsCalToDb == null)
        {
            logger.LogError("Unable to create object EarningsCalToDb");
            return;
        }
        if (finnhubCal == null || finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            logger.LogError("Vendor (Finnhub) did not provide any data to process");
            return;
        }
        var updateResult = await earningsCalToDb.UpdateFinnHubData(finnhubCal);
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
        services.AddScoped<EarningsCalToDb>();
        services.AddScoped<IHandleCache, HandleCache>();
    }
}