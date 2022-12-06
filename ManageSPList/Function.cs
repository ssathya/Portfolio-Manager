using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.Indexes;
using ManageSPList.Processing;
using ManageSPList.Processing.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace ManageSPList;

public class Function
{
    private const string ApplicationName = "SPIndustries";
    private ILogger<Function>? logger;

    public async Task FunctionHandler()
    {
        await DoApplicationProcessingAsync();
    }

    private async Task DoApplicationProcessingAsync()
    {
        IServiceCollection services = ServiceHandler.ConfigureServices(ApplicationName);
        AppSpecificSettings(services);
        ConnectToDb(services);
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<Function>>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }

        logger.LogInformation("Starting application");
        IBuildSP500Lst? buildSP500Lst = provider.GetService<IBuildSP500Lst>();
        if (buildSP500Lst != null)
        {
            logger.LogInformation("Starting to extract values from WebSite");
            List<IndexComponent> extractResult = await buildSP500Lst.ExcecAsync();
            if (extractResult != null && extractResult.Count >= 495)
            {
                await SaveValuesToDb(extractResult, provider);
            }
        }
        return;
    }

    private async Task<bool> SaveValuesToDb(List<IndexComponent> extractResult, ServiceProvider provider)
    {
        IHandleDataInDatabase handleDataInDatabase = provider.GetRequiredService<IHandleDataInDatabase>();
        if (handleDataInDatabase != null)
        {
            var processingResult = await handleDataInDatabase.ExecAsync(extractResult, IndexNames.SnP);
            return processingResult;
        }
        return false;
    }

    private static void AppSpecificSettings(IServiceCollection services)
    {
        services.AddScoped<IBuildSP500Lst, BuildSP500Lst>();
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
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
}