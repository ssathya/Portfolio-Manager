using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ApplicationModels.Indexes;
using ManageQQQList.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace ManageQQQList;

public class Function
{
    #region Private Fields

    private const string ApplicationName = "NASDAQIndustries";
    private ILogger<Function>? logger;

    #endregion Private Fields

    #region Public Methods

    public async Task FunctionHandler()
    {
        await DoApplicationProcessingAsync();
    }

    #endregion Public Methods

    #region Private Methods

    private void AppSpecificSettings(IServiceCollection services)
    {
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<BuildNasdaqLst>();
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
        BuildNasdaqLst? buildNasdaqLst = provider.GetService<BuildNasdaqLst>();
        if (buildNasdaqLst != null)
        {
            logger.LogInformation("Starting to extract values from WebSite");
            List<IndexComponent> extractResult = await buildNasdaqLst.ExecAsync();
            if (extractResult != null && extractResult.Count > 90)
            {
                bool saveResult = await SaveValuesToDb(extractResult, provider);
                if (saveResult)
                {
                    logger.LogInformation($"Nasdaq 100 extract Processing success");
                    return;
                }
                else
                {
                    logger.LogError($"Nasdaq 100 extract Processing failed in saving to database");
                    return;
                }
            }
            else
            {
                logger.LogError($"Nasdaq 100 extract Processing failed in data extraction");
                return;
            }
        }
        else
        {
            logger.LogError($"Nasdaq 100 application setup failed");
            return;
        }
    }

    private async Task<bool> SaveValuesToDb(List<IndexComponent> extractResult, ServiceProvider provider)
    {
        IHandleDataInDatabase handleDataInDatabase = provider.GetRequiredService<IHandleDataInDatabase>();
        if (handleDataInDatabase != null)
        {
            var processingResult = await handleDataInDatabase.ExecAsync(extractResult, IndexNames.Nasdaq);
            return processingResult;
        }
        return false;
    }

    #endregion Private Methods
}