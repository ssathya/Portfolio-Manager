using AppCommon.Services;
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

    private Task DoApplicationProcessingAsync()
    {
        IServiceCollection services = ServiceHandler.ConfigureServices(ApplicationName);
        ConnectToDb(services);
        return Task.CompletedTask;
    }

    private static void ConnectToDb(IServiceCollection services)
    {
        IConfiguration configuration = ServiceHandler.GetConfiguration(ApplicationName);
        services.AddDbContext<AppDbContext>(
            o => o.UseNpgsql(configuration["ConnectionString:DefaultConnection"])
            );
    }
}