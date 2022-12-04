using AppCommon.Services;
using ApplicationModels.Indexes;
using ManageSPList.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;

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
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<Function>>();
        if (logger == null)
        {
            Console.WriteLine("Unable to create logger object");
            return;
        }
        ConnectToDb(services);
        logger.LogInformation("Starting application");
        IBuildSP500Lst? buildSP500Lst = provider.GetService<IBuildSP500Lst>();
        if (buildSP500Lst != null)
        {
            logger.LogInformation("Starting to extract values from WebSite");
            List<IndexComponent> extractResult = await buildSP500Lst.ExcecAsync();
            if (extractResult != null && extractResult.Count >= 495)
            {
                await SaveValuesToDb(extractResult);
            }
        }
        return;
    }

    private Task SaveValuesToDb(List<IndexComponent> extractResult)
    {
        return Task.CompletedTask;
    }

    private static void AppSpecificSettings(IServiceCollection services)
    {
        services.AddScoped<IBuildSP500Lst, BuildSP500Lst>();
    }

    private static void ConnectToDb(IServiceCollection services)
    {
        IConfiguration configuration = ServiceHandler.GetConfiguration(ApplicationName);
        string? connectionStr = configuration["ConnectionString:DefaultConnection"];
        if (string.IsNullOrEmpty(connectionStr))
        {
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseNpgsql(connectionStr));
        }
    }
}