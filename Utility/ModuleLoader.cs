using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using WilsonEvoModuleLibrary.Hubs;
using WilsonEvoModuleLibrary.Interfaces;
using WilsonEvoModuleLibrary.Services;

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
        services.AddSingleton<IHubConnectionBuilder>(new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.Headers.Add("api-key", apiKey);
            }));                                    

        services.AddSingleton<ModuleClient>();
        services.AddSingleton<IModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        services.AddHostedService<ModuleClient>(provider => provider.GetRequiredService<ModuleClient>());


        services.AddSingleton<NodeServiceMapper>();
    }

    public static void LoadNodeServices(this IServiceCollection services)
    {
        List<Type> types = new();
        var currentDomain = AppDomain.CurrentDomain;
        var assemblies = currentDomain.GetAssemblies();
        foreach (var assembly in assemblies) types.AddRange(GetNodeService(assembly));



        WriteCoolDebug("Loading the services container:");
        foreach (var type in types)
            try
            {
                var serviceInterface = GetNodeServiceInterface(type);
                services.AddTransient(serviceInterface, type);
                WriteCoolDebug($"- ({serviceInterface.Name}){type.Name}", " Loaded", ConsoleColor.Green);
            }
            catch (Exception)
            {
                WriteCoolDebug($"- (...){type.Name}", " Error", ConsoleColor.DarkRed);
                throw;
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

    private static bool CheckType(Type type)
    {
        return type == typeof(NodeService<>) || type == typeof(NodeServices<,>) || type == typeof(AsyncNodeService<>) ||
               type == typeof(AsyncNodeServices<,>);
    }

    private static bool CheckInterfaceType(Type type)
    {
        return type == typeof(INodeService<>) || type == typeof(INodeServices<,>) ||
               type == typeof(IAsyncNodeService<>) || type == typeof(IAsyncNodeServices<,>);
    }

    public static IEnumerable<Type> GetNodeService(Assembly assembly)
    {
        return assembly.GetTypes().Where(
            t => t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType &&
                 CheckType(t.BaseType.GetGenericTypeDefinition()));
    }

    public static Type GetNodeServiceInterface(Type type)
    {
        return type.GetInterfaces().First(i => i.IsGenericType && CheckInterfaceType(i.GetGenericTypeDefinition()));
    }
}