using System;
using System.Data.Common;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Utility;

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
        _topicProcessor = client.CreateProcessor("broadcast", options.Value.ChannelName, new ServiceBusProcessorOptions() { AutoCompleteMessages = false });

        _topicProcessor.ProcessMessageAsync += HandleTopicAsync;
        _topicProcessor.ProcessErrorAsync += ProcessErrorAsync;
        _runProcessor.ProcessMessageAsync += HandleRunAsync;
        _runProcessor.ProcessErrorAsync += ProcessErrorAsync;
    }

    private async Task HandleTopicAsync(ProcessMessageEventArgs args)
    {
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        using var scope = _serviceProvider.CreateScope();


        var mapper = scope.ServiceProvider.GetRequiredService<NodeServiceMapper>();
        var updateRequest = BinarySerialization.Deserialize<UpdateModuleResponse>(args.Message.Body.ToArray());
        if (updateRequest != null)
            await mapper.UpdateService(updateRequest);
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

    private async Task HandleRunAsync(ProcessSessionMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<NodeServiceMapper>();
        var sender = scope.ServiceProvider.GetRequiredService<AzureBusSenderService>();
        var request = BinarySerialization.Deserialize<ServiceRequest>(args.Message.Body.ToArray());
        var response = await mapper.ExecuteService(request);
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

public sealed class AzureBusSenderService(ServiceBusClient serviceBusClient, IOptions<WilsonConfig> options)
{
    public async Task SendTriggerUpdateAsync(CancellationToken token = default)
    {
        var sender = serviceBusClient.CreateSender("broadcast");

        var message = new ServiceBusMessage()
        {
            // SessionId = ne,
            ApplicationProperties = { { "AccessToken", options.Value.Token },
            {
                "Name", "trigger"
            } },
        };

        await sender.SendMessageAsync(message, token);
    }
    public async Task UpdateConfigurationAsync(ModelsConfiguration configuration, CancellationToken token)
    {

        var data = BinarySerialization.Serialize(configuration, (settings) =>
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
        });
        var sender = serviceBusClient.CreateSender("moduleconfiguration");

        var message = new ServiceBusMessage()
        {
            // SessionId = ne,
            ApplicationProperties = { { "AccessToken", options.Value.Token } },
        };
        if (data != null)
        {
            message.Body = new BinaryData(data);
        }

        await sender.SendMessageAsync(message, token);
    }
    public async Task Response(ServiceResponse response, CancellationToken token)
    {
        var sender = serviceBusClient.CreateSender("responseworkflow");
        var binaryData = BinarySerialization.Serialize(response);
        var messageResponse = new ServiceBusMessage(BinaryData.FromBytes(binaryData))
        {
            SessionId = response.SessionData.Id
        };

        await sender.SendMessageAsync(messageResponse, token);
    }

    public async Task<Result> Start(string shortUrl, string channelName, object? request, CancellationToken token = default)
    {
        try
        {
            var rawBinary = BinarySerialization.Serialize(request);
            var sender = serviceBusClient.CreateSender("startworkflow");

            var message = new ServiceBusMessage()
            {
                SessionId = Guid.NewGuid().ToString(),
                ApplicationProperties = { { "shortUrl", shortUrl }, { "channelName", channelName } },
            };
            if (request != null)
            {
                message.Body = new BinaryData(rawBinary);
            }

            await sender.SendMessageAsync(message, token);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error communicating with the server: {ex.Message}");
        }
    }
    public async Task<Result> Run(string sessionId, object response, CancellationToken token = default)
    {
        try
        {
            var rawBin = response != null ? BinarySerialization.Serialize(response) : null;
            var sender = serviceBusClient.CreateSender("runworkflow");

            var message = new ServiceBusMessage(rawBin)
            {
                SessionId = sessionId
            };

            await sender.SendMessageAsync(message, token);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}

public sealed class ModuleClient
{
    private readonly ILogger _logger;
    private readonly AzureBusSenderService _busSender;

    public ModuleClient(
        ILogger logger,
        AzureBusSenderService busSender)
    {
        _logger = logger;
        _busSender = busSender;
    }

    public Task<Result> Next(string sessionId, object response, CancellationToken token = default)
    {
        return _busSender.Run(sessionId, response, token);
    }

    public Task<Result> Start(string shortUrl, string channelName, object? request, CancellationToken token = default)
    {
        return _busSender.Start(shortUrl, channelName, request, token);
    }

}
