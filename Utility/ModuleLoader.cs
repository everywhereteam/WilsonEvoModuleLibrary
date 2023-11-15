using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Hubs;
using WilsonEvoModuleLibrary.Network;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public static class ModuleLoader
{
    public static void AddWilsonCore(this IServiceCollection services, string apiKey)
    {

#if DEBUG
        var url = "https://localhost:7080/hub/module";
#else
        var url = "https://core.gestewwai.it/hub/module";
#endif
        //Console.SetOut(new LogTextWriter(Console.Out));
        string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        string sanitizedAppName = AppDomain.CurrentDomain.FriendlyName.Replace(" ", "_");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        Log.Information(@"
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

");

        //TODO: test
        services.AddSingleton<ILogger>(Log.Logger);
        services.LoadConfiguration();
        services.AddSingleton<ModuleClient>();
        services.AddSingleton<IModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddHostedService<ModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddSingleton<NodeServiceMapper>();
        services.AddSingleton<IHubConnectionBuilder>(new HubConnectionBuilder().WithUrl(url, options =>
        {          

            options.Transports = HttpTransportType.WebSockets;
            options.Headers.Add("api-key", apiKey);
        }).AddNewtonsoftJsonProtocol());
    }

    static void LoadConfiguration(this IServiceCollection services)
    {
        Log.Information("Loading service...");
        foreach (var type in GetNodeService(GetAssembliesWithoutModule()))
        {
            var serviceInterface = GetNodeServiceInterface(type);
            services.AddTransient(serviceInterface, type);
            Log.Information($"   -{type.Name}", " Loaded");
        }

        var configuration = new ModelsConfiguration();
        var map = new ServiceMappings();
        Log.Information("Loading task definitions...");
        configuration.Tasks = new Dictionary<string, TaskAttribute>();
        foreach (var type in GetTypesWithAttribute<TaskAttribute>(GetAssembliesWithoutModule()))
        {
            var task = type.GetCustomAttributes(typeof(TaskAttribute), false).Cast<TaskAttribute>().FirstOrDefault();
            configuration.Tasks.Add(type.Name, task);
            Log.Information($"   -{type.Name}", " Loaded");
        }

        Log.Information("Loading provider configuration...");
        foreach (var type in GetTypesWithAttribute<TaskProviderAttribute>(GetAssembliesWithoutModule()))
        {
            configuration.TaskProvider = type.GetCustomAttributes(typeof(TaskProviderAttribute), false)
                .Cast<TaskProviderAttribute>().FirstOrDefault();
            Log.Information($"   -{type.Name}", " Loaded");
        }

        Log.Information("Loading network definitions...");
        configuration.Network = new NetworkDefinition();   
        foreach (var type in GetNodeService(AppDomain.CurrentDomain.GetAssemblies()))
        {
            var interfaceService = ModuleLoader.GetNodeServiceInterface(type);
            var args = interfaceService.GenericTypeArguments;
            if (args.Length == 1)
            {
                map.ServiceMap.TryAdd(new MapPath(args[0].Name, string.Empty), interfaceService);
                configuration.Network.Network.Add(new NetworkNode(){TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName});
                Log.Information($"   -{args[0].Name}", " Loaded");
            }
            else if (args.Length == 2)
            {
                map.ServiceMap.TryAdd(new MapPath(args[0].Name, args[1].Name), interfaceService);
                configuration.Network.Network.Add(new NetworkNode() { TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName, ChannelControllerTypeName = args[1].Name , ChannelControllerTypeFullName = args[1].FullName });
                Log.Information($"   -{args[0].Name}.{args[1].Name}", " Loaded");
            }
        }
        services.AddSingleton(map);
        services.AddSingleton(configuration);
    }

    public static IEnumerable<Assembly> GetAssembliesWithoutModule()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        assemblies.Remove(Assembly.GetExecutingAssembly());
        return assemblies;
    }

    private static readonly Type[] ServiceType = { typeof(NodeService<>), typeof(NodeServices<,>), typeof(AsyncNodeService<>), typeof(AsyncNodeServices<,>) };
    private static readonly Type[] ServiceInterfaceType = { typeof(INodeService<>), typeof(INodeServices<,>), typeof(IAsyncNodeService<>), typeof(IAsyncNodeServices<,>) };


    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(IEnumerable<Assembly> assemblies) where TAttribute : Attribute
    {
        return assemblies.SelectMany(x => x.GetTypes()).Where(type => type.GetCustomAttributes(typeof(TAttribute), false).Length > 0);
    }

    public static IEnumerable<Type> GetNodeService(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.BaseType is { IsGenericType: true } &&
                        ServiceType.Contains(t.BaseType.GetGenericTypeDefinition()));
    }
    public static IEnumerable<Type> GetNodeServiceInterface(IEnumerable<Type> types)
    {
        return types.Select(x => x.GetInterfaces().First(i => i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition())));
    }

    public static Type GetNodeServiceInterface(Type type)
    {
        return type.GetInterfaces()
            .First(i => i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition()));
    }
}