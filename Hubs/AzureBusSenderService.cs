using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentResults;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WilsonEvoCoreLibrary.Core.Utility;
using WilsonEvoModuleLibrary.Configuration;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Hubs;

public sealed class AzureBusSenderService(ServiceBusClient serviceBusClient, IOptions<WilsonConfig> options)
{
    public async Task SendTriggerUpdateAsync(CancellationToken token = default)
    {
        var sender = serviceBusClient.CreateSender("broadcast");

        var message = new ServiceBusMessage
        {
            // SessionId = ne,
            ApplicationProperties = { { "AccessToken", options.Value.Token }, { "Name", "trigger" } }
        };

        await sender.SendMessageAsync(message, token);
    }

    public async Task UpdateConfigurationAsync(ModelsConfiguration configuration, CancellationToken token)
    {
        var data = BinarySerialization.Serialize(configuration, settings =>
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
        });
        var sender = serviceBusClient.CreateSender("moduleconfiguration");

        var message = new ServiceBusMessage
        {
            // SessionId = ne,
            ApplicationProperties = { { "AccessToken", options.Value.Token } }
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
        var messageResponse = new ServiceBusMessage(BinaryData.FromBytes(binaryData)) { SessionId = response.Session.SessionToken };

        await sender.SendMessageAsync(messageResponse, token);
    }

    public async Task<Result> Start(string shortUrl, string channelName, object? request, CancellationToken token = default)
    {
        try
        {
            var rawBinary = BinarySerialization.Serialize(request);
            var sender = serviceBusClient.CreateSender("startworkflow");

            var message = new ServiceBusMessage { SessionId = Guid.NewGuid().ToString(), ApplicationProperties = { { "shortUrl", shortUrl }, { "channelName", channelName } } };
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

            var message = new ServiceBusMessage(rawBin) { SessionId = sessionId };

            await sender.SendMessageAsync(message, token);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}