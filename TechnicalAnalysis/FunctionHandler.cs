using AppCommon.DatabaseHandler;
using AppCommon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;
using TechnicalAnalysis.Processing;
using TechnicalAnalysis.Processing.Fundamental;

namespace TechnicalAnalysis;

public class FunctionHandler
{
    private const string ApplicationName = "Compute";
    private ILogger<FunctionHandler>? logger;

    public async Task ExcecAsync()
    {
        await DoApplicationProcessingAsync();
    }

    private async Task DoApplicationProcessingAsync()
    {
        IServiceCollection services = ServiceHandler.ConfigureServices(ApplicationName);
        AppSpecificSettings(services);
        ConnectToDb(services);
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<FunctionHandler>>();
        TechAnalProcessing? techAnalProcessing = provider.GetService<TechAnalProcessing>();
        ScoreCompute? scoreCompute = provider.GetService<ScoreCompute>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }
        if (techAnalProcessing == null)
        {
            logger.LogError("Could not create object TechAnalProcessing");
            return;
        }
        if (scoreCompute == null)
        {
            logger.LogError("Could not create object ScoreCompute");
            return;
        }
        await techAnalProcessing.ExecAsync();
        await scoreCompute.ExecAsync();
        return;
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
        services.AddScoped<TechAnalProcessing>();
        services.AddScoped<ScoreCompute>();
    }
}