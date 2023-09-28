using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public static void AddWilsonCore(this IServiceCollection services, string url, string apiKey)
    {
        Console.WriteLine(@"
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
        services.LoadConfiguration();
        services.AddSingleton<ModuleClient>();
        services.AddSingleton<IModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddHostedService<ModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddSingleton<NodeServiceMapper>();
        services.AddSingleton<IHubConnectionBuilder>(new HubConnectionBuilder().WithUrl(url, options =>
        {
            options.Headers.Add("api-key", apiKey);
        }).AddNewtonsoftJsonProtocol(x=>{x.PayloadSerializerSettings.Error = (sender, args) =>  Console.WriteLine(args.ErrorContext.Error.Message);}));
    }

    static void LoadConfiguration(this IServiceCollection services)
    {
        WriteCoolDebug("Loading service...");
        foreach (var type in GetNodeService(GetAssembliesWithoutModule()))
        {
            var serviceInterface = GetNodeServiceInterface(type);
            services.AddTransient(serviceInterface, type);
            WriteCoolDebug($"   -{type.Name}", " Loaded", ConsoleColor.Green);
        }
        var configuration = new ModelsConfiguration();
        WriteCoolDebug("Loading task definitions...");
        configuration.Tasks = new Dictionary<string, TaskAttribute>(); 
        foreach (var type in GetTypesWithAttribute<TaskAttribute>(GetAssembliesWithoutModule()))
        {
            var task = type.GetCustomAttributes(typeof(TaskAttribute), false).Cast<TaskAttribute>().FirstOrDefault();
            configuration.Tasks.Add(type.FullName, task);
            WriteCoolDebug($"   -{type.Name}", " Loaded", ConsoleColor.Green);
        }
        WriteCoolDebug("Loading provider configuration...");
        foreach (var type in GetTypesWithAttribute<TaskProviderAttribute>(GetAssembliesWithoutModule()))
        {
            configuration.TaskProvider = type.GetCustomAttributes(typeof(TaskProviderAttribute), false).Cast<TaskProviderAttribute>().FirstOrDefault();
            WriteCoolDebug($"   -{type.Name}", " Loaded", ConsoleColor.Green);
        }
        WriteCoolDebug("Loading network definitions...");
        configuration.Network = new NetworkDefinition();
        configuration.Network.Network = new List<string>();
        foreach (var type in GetNodeService(AppDomain.CurrentDomain.GetAssemblies()))
        {
            var interfaceService = ModuleLoader.GetNodeServiceInterface(type);
            var args = interfaceService.GenericTypeArguments;
            if (args.Length == 1)
            {
                configuration.Network.Network.Add(args[0].Name);
                WriteCoolDebug($"   -{args[0].Name}", " Loaded", ConsoleColor.Green);
            }
            else if (args.Length == 2)
            {
                configuration.Network.Network.Add($"{args[0].Name}.{args[1].Name}");
                WriteCoolDebug($"   -{args[0].Name}.{args[1].Name}", " Loaded", ConsoleColor.Green);
            }
        }
        services.AddSingleton(configuration);
    }

    public static IEnumerable<Assembly> GetAssembliesWithoutModule()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        assemblies.Remove(Assembly.GetExecutingAssembly());
        return assemblies;
    }

    private static void WriteCoolDebug(string msg, string status = "", ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[EwModule] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(msg);
        Console.ForegroundColor = color;
        Console.WriteLine(status);
        Console.ResetColor();
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
        return type.GetInterfaces().First(i => i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition()));
    }
}