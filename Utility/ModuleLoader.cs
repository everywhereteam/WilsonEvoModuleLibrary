using System;
using System.Collections.Generic;
using System.IO;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Hubs;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public static class ModuleLoader
{
    private const string template = @"
         _       ___ __                    ___          _    __  ___          __      __        
        | |     / (_) /________  ____     /   |  ____  (_)  /  |/  /___  ____/ /_  __/ /__      
        | | /| / / / / ___/ __ \/ __ \   / /| | / __ \/ /  / /|_/ / __ \/ __  / / / / / _ \     
        | |/ |/ / / (__  ) /_/ / / / /  / ___ |/ /_/ / /  / /  / / /_/ / /_/ / /_/ / /  __/     
        |__/|__/_/_/____/\____/_/ /_/  /_/  |_/ .___/_/  /_/  /_/\____/\__,_/\__,_/_/\___/      
            ______                     _     /_/___                                  __         
           / ____/   _____  _______  _| |     / / /_  ___  ________     _____  _____/ /         
          / __/ | | / / _ \/ ___/ / / / | /| / / __ \/ _ \/ ___/ _ \   / ___/ / ___/ /          
         / /___ | |/ /  __/ /  / /_/ /| |/ |/ / / / /  __/ /  /  __/  (__  ) / /  / /           
        /_____/ |___/\___/_/   \__, / |__/|__/_/ /_/\___/_/   \___/  /____(_)_(_)/_(_)          
                              /____/

        ";

    public static void UseDefaultWilsonLogs(this IServiceCollection builder)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var sanitizedAppName = AppDomain.CurrentDomain.FriendlyName.Replace(" ", "_");
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Console().WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger();

//        builder.Host.UseSerilog((host, logger) =>
//        {
//#if DEBUG
//            logger.MinimumLevel.Verbose();
//#else
//            logger.MinimumLevel.Information();
//#endif
//            logger.WriteTo.Console();
//            logger.WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
//        });
    }

    public static void AddWilson(this IServiceCollection services, Action<(ModuleConfiguration Configuration, IServiceCollection Collection)> configuration = null)
    {
        services.UseDefaultWilsonLogs();


        //var moduleConfig = services.Configuration.GetSection("WilsonConfig").Get<WilsonConfig>() ?? new WilsonConfig();
        //services.Services.Configure<WilsonConfig>(services.Configuration.GetSection("WilsonConfig"));
        //if (string.IsNullOrWhiteSpace(moduleConfig.Token))
        //{
        //    throw new Exception("Missing token from the configuration, please setup the Appsettings with a valid token.");
        //}

        //Log.Information(template);
        services.AddSingleton<NodeServiceExecutor>();
        services.AddSingleton<ServiceProvider>();
        services.AddScoped<ModuleDeploymentService>();
        services.AddHostedService<ModuleConfigurationService>();
        var conf = new ModuleConfiguration();
        configuration?.Invoke((conf, services));
        ShowConfiguration(conf);

        services.AddSingleton(conf);
    }

    public static void Services(this (ModuleConfiguration Configuration, IServiceCollection Collection) builder, Action<ServiceRegistry> service)
    {
        var registry = new ServiceRegistry(builder.Collection);
        service?.Invoke(registry);
        builder.Configuration.ExecutorTypes = registry.ExecutorTypes;
    }
    public static void Token(this (ModuleConfiguration Configuration, IServiceCollection Collection) builder, string token)
    {
        builder.Configuration.Token = token;
    }

    public static void Tasks(this (ModuleConfiguration Configuration, IServiceCollection Collection) builder, Action<TaskRegistry> tasks)
    {
        tasks?.Invoke(builder.Configuration.TaskRegistry);
    }

    public class DeployRegistry
    {
        public Dictionary<Type, Type> Registry = new();
        public void Add<DeployService, TaskType>() where DeployService : IDeploymentHandler<TaskType> where TaskType : ITask
        {
            if (Registry.ContainsKey(typeof(TaskType)))
            {
                throw new Exception("Duplicate deploy handler");
            }
            Registry.Add(typeof(TaskType), typeof(IDeploymentHandler<TaskType>));
        }
    }

    public static void OnDeploy(this (ModuleConfiguration Configuration, IServiceCollection Collection) builder, Action<DeployRegistry> deployAction)
    {
        var registry = new DeployRegistry();
        deployAction?.Invoke(registry);
        builder.Configuration.DeploymentHandlers = registry.Registry;
    }

    public static void UseModuleConfiguration<TConfig>(this (ModuleConfiguration Configuration, IServiceCollection Collection) builder) where TConfig : IModuleConfiguration
    {
        builder.Configuration.ModuleConfigurationType = typeof(TConfig);
    }


    public static void UseAzureBusService(this (ModuleConfiguration Configuration, IServiceCollection Services) builder, Action<AzureBusSettings> settingsAction)
    {
        var settings = new AzureBusSettings();
        settingsAction?.Invoke(settings);


        var adminClient = new ServiceBusAdministrationClient(settings.ConnectionString);

        // Check if the subscription already exists
        if (!adminClient.SubscriptionExistsAsync("broadcast", settings.ProcessorName).GetAwaiter().GetResult())
        {
            adminClient.CreateSubscriptionAsync(new CreateSubscriptionOptions("broadcast", settings.ProcessorName)).GetAwaiter().GetResult();
            Console.WriteLine($"Subscription 'broadcast' created.");
        }
        else
        {
            Console.WriteLine($"Subscription 'broadcast' already exists.");
        }

        
        builder.Services.AddSingleton(new ServiceBusClient(settings.ConnectionString));
        builder.Services.AddHostedService<AzureBusReceiverService>();

        builder.Services
            .AddScoped<AzureBusSender>()
            .AddScoped<IModuleSender>(x => x.GetRequiredService<AzureBusSender>())
            .AddScoped<IUpdateModuleService>(x => x.GetRequiredService<AzureBusSender>());

        builder.Configuration.AzureBusSettings = settings;
    }

    

    private static void ShowConfiguration(ModuleConfiguration configuration)
    {
        Log.Information("Loading service...");
        foreach (var type in configuration.ExecutorTypes) Log.Information($"   -{type.Key.taskType} ", type.Key.channel);

        //TODO: show more
    }

    public class AzureBusSettings
    {
        public string ConnectionString { get; set; }
        public string ProcessorName { get; set; }
    }
}