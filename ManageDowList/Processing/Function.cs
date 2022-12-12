using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.Indexes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace ManageDowList.Processing;

public class Function
{
    #region Private Fields

    private const string ApplicationName = "Dow30Industries";
    private ILogger<Function>? logger;

    #endregion Private Fields

    #region Internal Methods

    internal async Task ExecuteAsync()
    {
        await DoApplicationProcessingAsync();
    }

    #endregion Internal Methods

    #region Private Methods

    private void AppSecificSettings(IServiceCollection services)
    {
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<BuildDowLst>();
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
        BuildDowLst? buildDowLst = provider.GetService<BuildDowLst>();
        if (buildDowLst == null)
        {
            logger.LogCritical("Unable to build service Build Dow List");
            return;
        }
        List<IndexComponent> extractResult = await buildDowLst.ExecAsync();
        if (extractResult.Count > 0)
        {
            bool saveResult = await SaveValuesToDb(extractResult, provider);
        }
    }

    private async Task<bool> SaveValuesToDb(List<IndexComponent> extractResult, ServiceProvider provider)
    {
        IHandleDataInDatabase handleDataInDatabase = provider.GetRequiredService<IHandleDataInDatabase>();
        if (handleDataInDatabase != null)
        {
            var processingResult = await handleDataInDatabase.ExecAsync(extractResult, IndexNames.Dow);
            return processingResult;
        }
        return false;
    }

    #endregion Private Methods
}