using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using ManageSimFinRatings.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageSimFinRatings;

public class ProcessHandler
{
    private const string ApplicationName = "SimFinHandler";
    private ILogger<ProcessHandler>? logger;

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
        logger = provider.GetService<ILogger<ProcessHandler>>();
        GetEntries? getEntries = provider.GetService<GetEntries>();
        ObtainSimFinRatings? obtainSimFinRatings = provider.GetService<ObtainSimFinRatings>();
        if (logger == null || getEntries == null || obtainSimFinRatings == null)
        {
            Console.WriteLine("Unable to establish dependency injection");
            return;
        }
        bool result = await getEntries.ExcecAsync();
        if (!result)
        {
            logger.LogError("Unable to get Entries from SimFin");
            return;
        }
        result = await obtainSimFinRatings.ExecAsync();
        if (!result)
        {
            logger.LogError("Unable to get SimFin Ratios");
            return;
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
        services.AddSingleton<GetEntries>();
        services.AddSingleton<ObtainSimFinRatings>();
    }
}