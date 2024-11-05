using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WilsonEvoCoreLibrary.Core.Utility;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services;

namespace WilsonEvoModuleLibrary.Hubs;

public sealed class AzureBusReceiverService : IHostedService
{
    private readonly ServiceBusSessionProcessor _runProcessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusProcessor _topicProcessor;
    
    public AzureBusReceiverService(ServiceBusClient client, IServiceProvider serviceProvider, IOptions<WilsonConfig> options)
    {
        _serviceProvider = serviceProvider;
        _runProcessor = client.CreateSessionProcessor(options.Value.ChannelName);
        _topicProcessor = client.CreateProcessor("broadcast", options.Value.ChannelName, new ServiceBusProcessorOptions { AutoCompleteMessages = false });

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
        using var scope = _serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<NodeServiceExecutor>();
        var sender = scope.ServiceProvider.GetRequiredService<AzureBusSenderService>();
        var request = BinarySerialization.Deserialize<ServiceRequest>(args.Message.Body.ToArray());
        var response = await mapper.HandleServiceRequest(request);
        await sender.Response(response, args.CancellationToken);
    }

    private async Task UpdateModuleConfigurationAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<ModelsConfiguration>();
        var sender = scope.ServiceProvider.GetRequiredService<AzureBusSenderService>();
        await sender.UpdateConfigurationAsync(config, token);
        await sender.SendTriggerUpdateAsync(token);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error in message processing: {args.Exception}");
        return Task.CompletedTask;
    }
}