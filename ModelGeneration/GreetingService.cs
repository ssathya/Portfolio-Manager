using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ModelGeneration;

public class GreetingService : IGreetingService
{
    private readonly ILogger<GreetingService> logger;
    private readonly IConfiguration config;

    public GreetingService(ILogger<GreetingService> logger, IConfiguration config)
    {
        this.logger = logger;
        this.config = config;
    }

    public void Run()
    {
        for (int i = 0; i < config.GetValue<int>("LoopTimes"); i++)
        {
            logger.LogInformation("Run number {runNumber}", i);
        }
    }
}