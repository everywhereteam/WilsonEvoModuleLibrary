using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using BlazorDynamicForm;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using WilsonEvoModuleLibrary.Common.Attributes;
using WilsonEvoModuleLibrary.Common.Entities.Configuration;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Hubs;
using WilsonEvoModuleLibrary.Network;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public static class ModuleLoader
{
    private static readonly Type[] ServiceType = [typeof(NodeService<>), typeof(AsyncNodeService<>)];

    private static readonly Type[] ServiceInterfaceType = [typeof(INodeService<>), typeof(IAsyncNodeService<>)];

    public static void UseWilsonDebug(this WebApplicationBuilder builder)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var sanitizedAppName = AppDomain.CurrentDomain.FriendlyName.Replace(" ", "_");
        Log.Logger = new LoggerConfiguration()
#if DEBUG || INTERNAL
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Console().WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger();

        builder.Host.UseSerilog((host, logger) =>
        {
#if DEBUG || INTERNAL
            logger.MinimumLevel.Verbose();
#else
            logger.MinimumLevel.Information();
#endif
            logger.WriteTo.Console();
            logger.WriteTo.File($"{logDirectory}/{sanitizedAppName}-" + ".txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
        });
    }

    public static IWilsonModuleBuilder UseWilsonCore(this WebApplicationBuilder builder, Action<JsonSerializerSettings> serializerSettings = null)
    {
        builder.UseWilsonDebug();
        var settings = new JsonSerializerSettings();
        serializerSettings?.Invoke(settings);
        WilsonSettings.NewtonsoftSerializer = JsonSerializer.Create(settings);

        var moduleConfig = builder.Configuration.GetSection("WilsonConfig").Get<WilsonConfig>() ?? new WilsonConfig();
        if (string.IsNullOrWhiteSpace(moduleConfig.Token))
        {
            throw new Exception("Missing token from the configuration, please setup the Appsettings with a valid token.");
        }

        var url = "";
        if (moduleConfig.IsDebug)
        {
            url = "https://wilsonevocore.azurewebsites.net/hub/module";
            Log.Information("Debug Mode enabled");
        }
        else
        {
            url = "https://wilsonevocore.azurewebsites.net/hub/module";
            Log.Information("Production Mode enabled");
        }


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
        builder.Services.AddSingleton<IModuleChannelClient>(provider => provider.GetRequiredService<ModuleClient>());
        builder.Services.AddHostedService(provider => provider.GetRequiredService<ModuleClient>());
        builder.Services.AddSingleton<NodeServiceMapper>();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IConfigStorageService, ConfigStorageService>();
        builder.Services.AddSingleton(new HubConnectionBuilder().WithUrl(url, options =>
        {
            options.Transports = HttpTransportType.WebSockets;
            //options.AccessTokenProvider??

            options.SkipNegotiation = true;
            options.ApplicationMaxBufferSize = 10_000_000;
            options.ClientCertificates = [];
            options.Cookies = new CookieContainer();
            options.CloseTimeout = TimeSpan.FromSeconds(5);
            options.DefaultTransferFormat = TransferFormat.Binary; //check the other one
            options.Credentials = null;
            options.Proxy = null;
            options.UseDefaultCredentials = true;
            options.TransportMaxBufferSize = 10_000_000;
            options.WebSocketConfiguration = null; //TOCHEKC
            options.WebSocketFactory = null;
            options.Headers.Add("api-key", moduleConfig.Token);
        }).AddMessagePackProtocol(conf =>
        {
            var resolver = CompositeResolver.Create(Array.Empty<IMessagePackFormatter>(), new IFormatterResolver[] { ContractlessStandardResolver.Instance });
            conf.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver).WithSecurity(MessagePackSecurity.UntrustedData).WithCompression(MessagePackCompression.Lz4Block).WithAllowAssemblyVersionMismatch(true).WithOldSpec().WithOmitAssemblyVersion(true);
        }).WithAutomaticReconnect(new CustomRetryPolicy()).ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);

#if DEBUG
            logging.SetMinimumLevel(LogLevel.Trace);
#endif
            logging.AddConsole();
            logging.AddSerilog();
        }));

        return new WilsonModuleBuilder(builder.Services);
    }


    public interface IWilsonModuleBuilder
    {
        IWilsonModuleBuilder AddTask<T>() where T : class, INodeService;
        IWilsonModuleBuilder AddAsyncTask<T>() where T : class, IAsyncNodeService;
        IWilsonModuleBuilder AddChannelTask<T>(string channelName) where T : class, INodeService;
        IWilsonModuleBuilder AddChannelAsyncTask<T>(string channelName) where T : class, IAsyncNodeService;
        IWilsonModuleBuilder AddChannelTask<T,C>() where T : class, INodeService;
        IWilsonModuleBuilder AddChannelAsyncTask<T,C>() where T : class, IAsyncNodeService;

        IWilsonModuleBuilder AddConfiguration<T>() where T : class, new();

    }

    public record ChannelService(Type Type, string ChannelName);
    private class WilsonModuleBuilder(IServiceCollection services) : IWilsonModuleBuilder
    {
        private List<ChannelService> _services = new();
        private Type? _configuration; 
        public IWilsonModuleBuilder AddTask<T>() where T : class, INodeService
        {
            AddTaskService<T>();
            return this;
        }

        public IWilsonModuleBuilder AddAsyncTask<T>() where T : class, IAsyncNodeService
        {
            AddTaskService<T>();
            return this;
        }

        public IWilsonModuleBuilder AddChannelTask<T>(string channelName) where T : class, INodeService
        {
            AddChannelTaskService<T>(channelName);
            return this;
        }

        public IWilsonModuleBuilder AddChannelAsyncTask<T>(string channelName) where T : class, IAsyncNodeService
        {
            AddChannelTaskService<T>(channelName);
            return this;
        }

        public IWilsonModuleBuilder AddChannelTask<T, C>() where T : class, INodeService
        {
            AddChannelTaskService<T>(nameof(C));
            return this;
        }

        public IWilsonModuleBuilder AddChannelAsyncTask<T, C>() where T : class, IAsyncNodeService
        {
            AddChannelTaskService<T>(nameof(C));
            return this;
        }

        private void AddTaskService<T>()
        {
            _services.Add(new ChannelService(typeof(T), string.Empty));
        }

        private void AddChannelTaskService<T>(string channelName)
        {
            _services.Add(new ChannelService(typeof(T), channelName));
        }

        public IWilsonModuleBuilder AddConfiguration<T>() where T : class, new()
        {
            _configuration = typeof(T);
            return this;
        }

        protected void Build()
        {
            var configuration = new ModelsConfiguration();
            var map = new ServiceMappings();

            Log.Information("Loading task definitions...");
           
            foreach (var type in GetTypesWithAttribute<TaskAttribute>(GetAssembliesWithoutModule()))
            {
                var task = type.GetCustomAttributes(typeof(TaskAttribute), false).Cast<TaskAttribute>().FirstOrDefault();
                if (task != null)
                {
                    task.Definition = DataAnnotationParser.ReadDataAnnotations(type);
                    configuration.Tasks.Add(type.Name, task);
                    Log.Information($"   -{type.Name}", " Loaded");
                }
            }

            Log.Information("Loading provider configuration...");
            if (_configuration != null)
            {
                configuration.ConfigurationScheme = DataAnnotationParser.ReadDataAnnotations(_configuration);
                Log.Information($"   -{_configuration.Name}", " Loaded");
            }

            Log.Information("Loading service...");

            foreach (var service in _services)
            {
                var serviceInterface = GetNodeServiceInterface(service.Type);
                services.AddTransient(serviceInterface, service.Type);
                var args = serviceInterface.GenericTypeArguments;
                if (args.Length == 1)
                {
                    map.ServiceMap.TryAdd(new MapPath(args[0].Name, service.ChannelName), serviceInterface);
                    configuration.Network.Add(new NetworkNode { TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName, ChannelControllerTypeName = service.ChannelName, ChannelControllerTypeFullName = service.ChannelName });

                    Log.Information($"   -{args[0].Name}", " Loaded");
                }
                Log.Information($"   -{service.Type.Name}", " Loaded");
            }

            services.AddSingleton(map);
            services.AddSingleton(configuration);
        }
    }



    private static void LoadConfiguration(this IServiceCollection services)
    {
        //Log.Information("Loading service...");
        //foreach (var type in GetNodeService(GetAssembliesWithoutModule()))
        //{
        //    var serviceInterface = GetNodeServiceInterface(type);
        //    services.AddTransient(serviceInterface, type);
        //    Log.Information($"   -{type.Name}", " Loaded");
        //}

        //var configuration = new ModelsConfiguration();
        //var map = new ServiceMappings();
        //Log.Information("Loading task definitions...");
        //configuration.Tasks = new Dictionary<string, TaskAttribute>();
        //foreach (var type in GetTypesWithAttribute<TaskAttribute>(GetAssembliesWithoutModule()))
        //{
        //    var task = type.GetCustomAttributes(typeof(TaskAttribute), false).Cast<TaskAttribute>().FirstOrDefault();
        //    task.Definition = DataAnnotationParser.ReadDataAnnotations(type);
        //    configuration.Tasks.Add(type.Name, task);
        //    Log.Information($"   -{type.Name}", " Loaded");
        //}

        //Log.Information("Loading provider configuration...");
        //foreach (var type in GetTypesWithAttribute<TaskProviderAttribute>(GetAssembliesWithoutModule()))
        //{
        //    configuration.TaskProvider = type.GetCustomAttributes(typeof(TaskProviderAttribute), false).Cast<TaskProviderAttribute>().FirstOrDefault();
        //    configuration.TaskProvider.Definition = DataAnnotationParser.ReadDataAnnotations(type);
        //    Log.Information($"   -{type.Name}", " Loaded");
        //}

        //Log.Information("Loading network definitions...");
        //configuration.Network = new NetworkDefinition();
        //foreach (var type in GetNodeService(AppDomain.CurrentDomain.GetAssemblies()))
        //{
        //    var interfaceService = GetNodeServiceInterface(type);
        //    var args = interfaceService.GenericTypeArguments;
        //    if (args.Length == 1)
        //    {
        //        map.ServiceMap.TryAdd(new MapPath(args[0].Name, string.Empty), interfaceService);
        //        configuration.Network.Network.Add(new NetworkNode { TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName });
        //        Log.Information($"   -{args[0].Name}", " Loaded");
        //    }
        //    else if (args.Length == 2)
        //    {
        //        map.ServiceMap.TryAdd(new MapPath(args[0].Name, args[1].Name), interfaceService);
        //        configuration.Network.Network.Add(new NetworkNode { TaskTypeName = args[0].Name, TaskTypeFullName = args[0].FullName, ChannelControllerTypeName = args[1].Name, ChannelControllerTypeFullName = args[1].FullName });
        //        Log.Information($"   -{args[0].Name}.{args[1].Name}", " Loaded");
        //    }
        //}

        //services.AddSingleton(map);
        //services.AddSingleton(configuration);
    }

    public static IEnumerable<Assembly> GetAssembliesWithoutModule()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        assemblies.Remove(Assembly.GetExecutingAssembly());
        return assemblies;
    }


    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(IEnumerable<Assembly> assemblies) where TAttribute : Attribute
    {
        return assemblies.SelectMany(x => x.GetTypes()).Where(type => type.GetCustomAttributes(typeof(TAttribute), false).Length > 0);
    }

    public static IEnumerable<Type> GetNodeService(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(assembly => assembly.GetTypes()).Where(t => t.IsClass && !t.IsAbstract && t.BaseType is { IsGenericType: true } && ServiceType.Contains(t.BaseType.GetGenericTypeDefinition()));
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