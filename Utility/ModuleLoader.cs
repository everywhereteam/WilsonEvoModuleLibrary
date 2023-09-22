using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Hubs;
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
        services.LoadNodeServices();
        services.LoadNodeConfiguration();
        services.AddSingleton<IHubConnectionBuilder>(new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.Headers.Add("api-key", apiKey);
            }).AddNewtonsoftJsonProtocol());

        services.AddSingleton<ModuleClient>();
        services.AddSingleton<IModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddHostedService<ModuleClient>(provider => provider.GetRequiredService<ModuleClient>());


        services.AddSingleton<NodeServiceMapper>();
    }

    static void LoadNodeConfiguration(this IServiceCollection services)
    {
        
        var configuration = new ModelsConfiguration();
        WriteCoolDebug("Loading task definitions...");
        foreach (var type in GetTypesWithAttribute<TaskAttribute>())
        {
            var task = type.GetCustomAttributes(typeof(TaskAttribute), false).Cast<TaskAttribute>().FirstOrDefault();

            configuration.Tasks.Add(type.FullName,task);

            WriteCoolDebug($"- {type.Name}", " Loaded", ConsoleColor.Green);
        }
        WriteCoolDebug("Loading provider configuration...");
        foreach (var type in GetTypesWithAttribute<TaskProviderAttribute>())
        {
            configuration.TaskProvider = type.GetCustomAttributes(typeof(TaskProviderAttribute), false).Cast<TaskProviderAttribute>().FirstOrDefault();
            //m.. shit?
            WriteCoolDebug($"- {type.Name}", " Loaded", ConsoleColor.Green);
        }


        services.AddSingleton(configuration);
    }

    static void LoadNodeServices(this IServiceCollection services)
    {
        List<Type> types = new();
        var currentDomain = AppDomain.CurrentDomain;
        var assemblies = currentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies) types.AddRange(GetNodeService(assembly));

        WriteCoolDebug("Loading service...");
        foreach (var type in types)
        {
            try
            {
                var serviceInterface = GetNodeServiceInterface(type);
                services.AddTransient(serviceInterface, type);
                WriteCoolDebug($"- ({serviceInterface.Name}){type.Name}{type.AssemblyQualifiedName}", " Loaded", ConsoleColor.Green);
            }
            catch (Exception)
            {
                WriteCoolDebug($"- (...){type.Name}", " Error", ConsoleColor.DarkRed);
                throw;
            }
        }
    }

    private static void WriteCoolDebug(string msg, string status = "", ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[NodeService] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(msg);
        Console.ForegroundColor = color;
        Console.WriteLine(status);
        Console.ResetColor();
    }

    private static readonly Type[] ServiceType = { typeof(NodeService<>), typeof(NodeServices<,>), typeof(AsyncNodeService<>), typeof(AsyncNodeServices<,>) };
    private static readonly Type[] ServiceInterfaceType = { typeof(INodeService<>), typeof(INodeServices<,>), typeof(IAsyncNodeService<>), typeof(IAsyncNodeServices<,>) };


    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
    public static IEnumerable<Type> GetNodeService(Assembly assembly)
    {
        return assembly.GetTypes().Where(
            t => t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType &&
                 ServiceType.Contains(t.BaseType.GetGenericTypeDefinition()));
    }

    public static Type GetNodeServiceInterface(Type type)
    {
        return type.GetInterfaces().First(i => i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition()));
    }
}