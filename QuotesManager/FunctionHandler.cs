using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;
using QuotesManager.Processing;

namespace QuotesManager;

public class FunctionHandler
{
    private const string ApplicationName = "ObtainHistoricPrice";
    private ILogger<FunctionHandler>? logger;

    public async Task ExecAsync()
    {
        await DoApplicationProcessingAsync();
    }

    private async Task DoApplicationProcessingAsync()
    {
        IServiceCollection services = ServiceHandler.ConfigureServices(ApplicationName);
        AppSpecificSettings(services);
        ConnectToDb(services);
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<FunctionHandler>>();
        GetValuesFromYahoo? getValuesFromYahoo = provider.GetService<GetValuesFromYahoo>();
        QuotesDbProcessing? quotesDbProcessing = provider.GetService<QuotesDbProcessing>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }
        if (getValuesFromYahoo == null)
        {
            logger.LogCritical("Unable to create object GetValuesFromYahoo");
            return;
        }
        if (quotesDbProcessing == null)
        {
            logger.LogCritical("Unable to create object QuotesDbProcessing");
            return;
        }
        List<YPrice> quotes = await getValuesFromYahoo.ExecAsync();
        if (quotes == null || quotes.Count == 0)
        {
            logger.LogCritical("Unable to obtain quotes from Yahoo");
            return;
        }
        var updateResult = await quotesDbProcessing.ExecAsync(quotes);
        logger.LogInformation($"Getting quotes {(updateResult ? "success" : "failed")}");
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

    private void AppSpecificSettings(IServiceCollection services)
    {
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IHandleCache, HandleCache>();
        services.AddScoped<GetValuesFromYahoo>();
        services.AddScoped<QuotesDbProcessing>();
    }
}