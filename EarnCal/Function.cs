using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.EarningsCal;
using EarnCal.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace EarnCal;

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
        ConsumeYahooEc? consumeYahooEc = provider.GetService<ConsumeYahooEc>();

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
        if (earningsCalToDb == null)
        {
            logger.LogError("Unable to create object EarningsCalToDb");
            return;
        }
        if (consumeYahooEc == null)
        {
            logger.LogError("Unable to create ConsumeYahooEc object..");
            return;
        }
        FinnhubCal? finnhubCal = await consumeFinnhubCalendar.GetValuesFromVendor();
        List<YahooEarningCal> earningsDates = await consumeYahooEc.GetValuesFromYahoo();
        if (finnhubCal == null || finnhubCal.EarningsCalendar == null || finnhubCal.EarningsCalendar.Length == 0)
        {
            logger.LogError("Vendor (Finnhub) did not provide any data to process");
            return;
        }
        var updateResult1 = await earningsCalToDb.UpdateFinnHubData(finnhubCal);
        var updateResult2 = await earningsCalToDb.UpdateYahooEarningsCal(earningsDates);
        logger.LogInformation($"Processing was {((updateResult1 & updateResult2) ? "Success" : "Failed")}");
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
        services.AddScoped<ConsumeFinnhubCalendar>();
        services.AddScoped<ConsumeYahooEc>();
        services.AddScoped<EarningsCalToDb>();
        services.AddScoped<IHandleCache, HandleCache>();
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
    }
}