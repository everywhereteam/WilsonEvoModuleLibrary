using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WilsonEvoCoreLibrary.Core.Utility;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services;

namespace WilsonEvoModuleLibrary.Hubs;

public interface IUpdateModuleService
{
    Task UpdateModuleConfigurationAsync(ModuleConfiguration configuration, CancellationToken token);
}

public sealed class ModuleConfigurationService( IServiceProvider serviceProvider, ModuleConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<IUpdateModuleService>();
        await sender.UpdateModuleConfigurationAsync(configuration, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}


public sealed class AzureBusReceiverService : IHostedService
{
    private readonly ServiceBusSessionProcessor _runProcessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusProcessor _topicProcessor;

    public AzureBusReceiverService(ServiceBusClient client, IServiceProvider serviceProvider, ModuleConfiguration configuration)
    {
        var config = new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = 100, // Ensure only one session is processed at a time
            MaxConcurrentCallsPerSession = 1, // Ensure only one message is processed per session at a time
            AutoCompleteMessages = false // Allows you to control when messages are marked as complete
        };
        _serviceProvider = serviceProvider;
        _runProcessor = client.CreateSessionProcessor(configuration.AzureBusSettings.ProcessorName, config);

        _topicProcessor = client.CreateProcessor("broadcast", configuration.AzureBusSettings.ProcessorName, new ServiceBusProcessorOptions { AutoCompleteMessages = false });

        _topicProcessor.ProcessMessageAsync += HandleTopicAsync;
        _topicProcessor.ProcessErrorAsync += ProcessErrorAsync;
        _runProcessor.ProcessMessageAsync += HandleRunAsync;
        _runProcessor.ProcessErrorAsync += ProcessErrorAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _topicProcessor.StartProcessingAsync(cancellationToken);
        await _runProcessor.StartProcessingAsync(cancellationToken);
        await UpdateModuleConfigurationAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _runProcessor.StopProcessingAsync(cancellationToken);
        await _topicProcessor.StopProcessingAsync(cancellationToken);
    }

    private async Task HandleTopicAsync(ProcessMessageEventArgs args)
    {
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        using var scope = _serviceProvider.CreateScope();


        var updateService = scope.ServiceProvider.GetRequiredService<ModuleDeploymentService>();
        var updateRequest = BinarySerialization.Deserialize<UpdateModuleResponse>(args.Message.Body.ToArray());
        if (updateRequest != null)
        {
            await updateService.DeployModulesAsync(updateRequest);
        }
    }

    private async Task HandleRunAsync(ProcessSessionMessageEventArgs args)
    {
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);

        using var scope = _serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<NodeServiceExecutor>();
        var sender = scope.ServiceProvider.GetRequiredService<AzureBusSender>();
        var request = BinarySerialization.Deserialize<ServiceRequest>(args.Message.Body.ToArray());
        var response = await mapper.HandleServiceRequest(request);
        await sender.Response(response, args.CancellationToken);
    }

    public async Task UpdateModuleConfigurationAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<ModuleConfiguration>();
        var sender = scope.ServiceProvider.GetRequiredService<AzureBusSender>();
        //await sender.UpdateConfigurationAsync(config, token);
        await sender.SendTriggerUpdateAsync(config, token);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error in message processing: {args.Exception}");
        return Task.CompletedTask;
    }
}