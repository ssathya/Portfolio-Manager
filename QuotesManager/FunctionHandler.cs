﻿using AppCommon.CacheHandler;
using AppCommon.DatabaseHandler;
using AppCommon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using PsqlAccess.SecListMaintain;
using QuotesManager.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        ServiceProvider provider = services.BuildServiceProvider();
        logger = provider.GetService<ILogger<FunctionHandler>>();
        GetValuesFromYahoo? getValuesFromYahoo = provider.GetService<GetValuesFromYahoo>();
        if (getValuesFromYahoo != null)
        {
            await getValuesFromYahoo.ExecAsync();
        }
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
    }
}