using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using EarningsReport.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;

namespace EarningsReport;

public class Function
{
    private const string ApplicationName = "EarningsReport";
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
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        ServiceProvider provider = services.BuildServiceProvider();

        logger = provider.GetService<ILogger<Function>>();
        GetFinancialStatements? getFinancialStatements = provider.GetService<GetFinancialStatements>();
        FinStatementsToDb? finStatementsToDb = provider.GetService<FinStatementsToDb>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }

        logger.LogInformation("Starting application");
        if (getFinancialStatements == null)
        {
            logger.LogError("Unable to create object GetFinancialStatements");
            return;
        }
        if (finStatementsToDb == null)
        {
            logger.LogError("Unable to create object FinStatementsToDb");
            return;
        }
        var finStatements = await getFinancialStatements.ExcecAsync();
        if (finStatements.Count != 0)
        {
            var insertResult = await finStatementsToDb.ExcecAsync(finStatements);
            if (!insertResult)
            {
                logger.LogError("Unable to update database");
            }
        }
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

    private void AppSecificSettings(IServiceCollection services)
    {
        services.AddScoped<IHandleCache, HandleCache>();
        services.AddScoped<IHandleDataInDatabase, HandleDataInDatabase>();
        services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<GetFinancialStatements>();
        services.AddScoped<FinStatementsToDb>();
    }
}