using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Hubs;

public sealed class ModuleClient : IHostedService, IModuleClient
{
    private readonly byte[]? _configurationRaw;
    private readonly HubConnection _connection;
    private readonly ILogger _logger;
    private readonly NodeServiceMapper _mapper;

    public ModuleClient(ILogger logger, ModelsConfiguration configuration, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper, IHostApplicationLifetime hostApplicationLifetime)
    {
        _mapper = mapper;
        _logger = logger;

        _configurationRaw = BinarySerialization.Serialize(configuration, settings =>
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
        });

        _connection = hubConnectionBuilder.Build();
        _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);
        _connection.On<UpdateRequest, string>(nameof(EnvironmentUpdate), EnvironmentUpdate);
        _connection.Closed += Closed;
        _connection.ServerTimeout = TimeSpan.FromSeconds(25);
        _connection.HandshakeTimeout = TimeSpan.FromSeconds(25);
        _connection.Reconnected += OnReconnect;
        _connection.Reconnecting += OnReconnecting;
        _connection.KeepAliveInterval = TimeSpan.FromSeconds(10);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
            try
            {
                _logger.Information("[WilsonModule] Connecting with the server..");
                await _connection.StartAsync(cancellationToken);
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("RegisterServices", _configurationRaw, cancellationToken);

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[WilsonModule] Trying to reconnect with the server: \n {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                await _connection.StopAsync(cancellationToken);
            }
    }


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Warning("[WilsonModule] Closing Connection..");
        await _connection.DisposeAsync();
    }

    public async Task<ServiceResponse> Execute(ServiceRequest request)
    {
        await _mapper.ExecuteService(request.SessionData, request.Type, request.SessionData.ChannelType, request.NodeData);
        var serviceResponse = new ServiceResponse { SessionData = request.SessionData };
        return serviceResponse;
    }

    private Task OnReconnecting(Exception? arg)
    {
        _logger.Error($"[WilsonModule] Reconnecting to the server.. {arg?.Message} \n {arg?.StackTrace}");
        return Task.CompletedTask;
    }

    private async Task OnReconnect(string? arg)
    {
        _logger.Information($"[WilsonModule] Reconnected with the server. {arg}");
        await _connection.InvokeAsync("RegisterServices", _configurationRaw);
    }

    private async Task<string> EnvironmentUpdate(UpdateRequest updateRequest)
    {
        return await _mapper.UpdateService(updateRequest);
    }

    public async Task<Result<SessionData>> Start(object channel, string envCode, SessionData? session = null, CancellationToken token = default)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            return Result.Fail($"Error the module was waiting to connect with the server: {_connection.State.ToString()}");
        }

        session ??= new SessionData();
        session.ChannelType = channel.GetType().Name;
        session.CurrentShortUrl = envCode;
        try
        {
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error with the module on the server communication on Start: {ex.Message} \n {ex.StackTrace} ");
        }
    }

    public async Task<Result<SessionData>> Next(string sessionId, object? request, CancellationToken token = default)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            return Result.Fail($"Error the module was waiting to connect with the server: {_connection.State.ToString()}");
        }

        try
        {
            var rawBin = request != null ? BinarySerialization.Serialize(request) : null;
            var response = await _connection.InvokeAsync<SessionData>("Next", sessionId, rawBin, token);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error with the module on the server communication on Next: {ex.Message} \n {ex.StackTrace} ");
        }
    }

    private Task Closed(Exception? error)
    {
        _logger.Warning($"[WilsonModule] Connection closed with the master. \n {error?.Message} \n {error?.StackTrace}");
        return Task.CompletedTask;
    }
}