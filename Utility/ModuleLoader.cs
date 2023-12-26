using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BlazorDynamicForm;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Hubs;
using WilsonEvoModuleLibrary.Network;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public static class ModuleLoader
{
    private static readonly Type[] ServiceType =
        { typeof(NodeService<>), typeof(NodeServices<,>), typeof(AsyncNodeService<>), typeof(AsyncNodeServices<,>) };

    private static readonly Type[] ServiceInterfaceType =
    {
        typeof(INodeService<>), typeof(INodeServices<,>), typeof(IAsyncNodeService<>), typeof(IAsyncNodeServices<,>)
    };

    public static void UseWilsonDebug(this WebApplicationBuilder builder)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var sanitizedAppName = AppDomain.CurrentDomain.FriendlyName.Replace(" ", "_");
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif                                  
            .WriteTo.Console()
            .WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        builder.Host.UseSerilog((host, logger) =>
        {
#if DEBUG
            logger.MinimumLevel.Verbose();
#else
            logger.MinimumLevel.Information();
#endif
            logger.WriteTo.Console();
            logger.WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);
        });
    }

    public static void UseWilsonCore(this WebApplicationBuilder builder, Action<JsonSerializerSettings> serializerSettings = null)
    {
        builder.UseWilsonDebug();
        var settings = new JsonSerializerSettings();
        serializerSettings?.Invoke(settings);
        WilsonSettings.NewtonsoftSerializer = JsonSerializer.Create(settings);

        var moduleConfig = builder.Configuration.GetSection("WilsonConfig").Get<WilsonConfig>() ?? new WilsonConfig();
        if (string.IsNullOrWhiteSpace(moduleConfig.Token))
            throw new Exception("Missing token from the configuration, please setup the Appsettings with a valid token.");

#if DEBUG
        var url = "https://localhost:44335/hub/module";
#else
        var url = "https://core.gestewwai.it/hub/module";
        //url = "https://localhost:44335/hub/module";
#endif




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


        builder.Services.LoadConfiguration();
        builder.Services.AddSingleton<ModuleClient>();
        builder.Services.AddSingleton<IModuleClient>(provider => provider.GetRequiredService<ModuleClient>());
        builder.Services.AddHostedService(provider => provider.GetRequiredService<ModuleClient>());
        builder.Services.AddSingleton<NodeServiceMapper>();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IConfigStorageService, ConfigStorageService>();
        builder.Services.AddSingleton(new HubConnectionBuilder().WithUrl(url, options =>
        {
            options.Transports = HttpTransportType.WebSockets;
            // options.AccessTokenProvider??

            options.SkipNegotiation = true;
            options.ApplicationMaxBufferSize = 10_000_000;
            options.ClientCertificates = new X509CertificateCollection();
            options.Cookies = new CookieContainer();
            options.CloseTimeout = TimeSpan.FromSeconds(5);
            options.DefaultTransferFormat = TransferFormat.Text; //check the other one
            options.Credentials = null;
            options.Proxy = null;
            options.UseDefaultCredentials = true;
            options.TransportMaxBufferSize = 10_000_000;
            options.WebSocketConfiguration = null; //TOCHEKC
            options.WebSocketFactory = null;
            options.Headers.Add("api-key", moduleConfig.Token);

        }).ConfigureLogging((logging) =>
        {
#if DEBUG
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddConsole();
#endif
        }).AddMessagePackProtocol(conf =>
        {
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                 new IMessagePackFormatter[] { },
                 new IFormatterResolver[]
                 {
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance
                 });
            conf.SerializerOptions = MessagePackSerializerOptions.Standard
             .WithResolver(resolver)
                .WithSecurity(MessagePackSecurity.UntrustedData)
                .WithCompression(MessagePackCompression.Lz4Block)
                .WithAllowAssemblyVersionMismatch(true)
                .WithOldSpec()
                .WithOmitAssemblyVersion(true);
        }));
    }

    private static void LoadConfiguration(this IServiceCollection services)
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
            task.Definition = DataAnnotationParser.ReadDataAnnotations(type);
            configuration.Tasks.Add(type.Name, task);
            Log.Information($"   -{type.Name}", " Loaded");
        }
        //TODO: MISSING CONFIGURATION LOADGIN
        Log.Information("Loading provider configuration...");
        foreach (var type in GetTypesWithAttribute<TaskProviderAttribute>(GetAssembliesWithoutModule()))
        {
            configuration.TaskProvider = type.GetCustomAttributes(typeof(TaskProviderAttribute), false)
                .Cast<TaskProviderAttribute>().FirstOrDefault();
            configuration.TaskProvider.Definition = DataAnnotationParser.ReadDataAnnotations(type);
            Log.Information($"   -{type.Name}", " Loaded");
        }

        Log.Information("Loading network definitions...");
        configuration.Network = new NetworkDefinition();
        foreach (var type in GetNodeService(AppDomain.CurrentDomain.GetAssemblies()))
        {
            var interfaceService = GetNodeServiceInterface(type);
            var args = interfaceService.GenericTypeArguments;
            if (args.Length == 1)
            {
                map.ServiceMap.TryAdd(new MapPath(args[0].Name, string.Empty), interfaceService);
                configuration.Network.Network.Add(new NetworkNode
                { TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName });
                Log.Information($"   -{args[0].Name}", " Loaded");
            }
            else if (args.Length == 2)
            {
                map.ServiceMap.TryAdd(new MapPath(args[0].Name, args[1].Name), interfaceService);
                configuration.Network.Network.Add(new NetworkNode
                {
                    TaskTypeName = args[0].Name,
                    TaskTypeFullName = args[0].FullName,
                    ChannelControllerTypeName = args[1].Name,
                    ChannelControllerTypeFullName = args[1].FullName
                });
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


    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(IEnumerable<Assembly> assemblies)
        where TAttribute : Attribute
    {
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(type => type.GetCustomAttributes(typeof(TAttribute), false).Length > 0);
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
        return types.Select(x =>
            x.GetInterfaces().First(i =>
                i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition())));
    }

    public static Type GetNodeServiceInterface(Type type)
    {
        return type.GetInterfaces()
            .First(i => i.IsGenericType && ServiceInterfaceType.Contains(i.GetGenericTypeDefinition()));
    }
}