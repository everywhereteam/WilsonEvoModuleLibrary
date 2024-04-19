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
    private readonly ModelsConfiguration _configuration;
    private readonly HubConnection _connection;
    private readonly ILogger _logger;
    private readonly NodeServiceMapper _mapper;

    private readonly byte[]? ConfigurationRaw;

    public ModuleClient(ILogger logger, ModelsConfiguration configuration,
        IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;

        ConfigurationRaw = BinarySerialization.Serialize(_configuration, (settings) =>
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
        });

        _connection = hubConnectionBuilder.WithAutomaticReconnect(new CustomRetryPolicy()).Build();
        _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);
        _connection.On<UpdateRequest, string>("EnvironmentUpdate", EnvironmentUpdate);
        _connection.Closed += Closed;
        _connection.ServerTimeout = TimeSpan.FromSeconds(25);
        _connection.HandshakeTimeout = TimeSpan.FromSeconds(25);
        _connection.Reconnected += OnReconnect;
        // _connection.KeepAliveInterval = TimeSpan.FromSeconds(10);
    }

    private async Task OnReconnect(string? arg)
    {
        await _connection.InvokeAsync("RegisterServices", ConfigurationRaw);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
            try
            {
                await _connection.StartAsync(cancellationToken);
                if (_connection.State == HubConnectionState.Connected)
                {
                   
                    await _connection.InvokeAsync("RegisterServices", ConfigurationRaw, cancellationToken);

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                await _connection.StopAsync(cancellationToken);
            }
    }


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Warning("[Wilson] Closing Connection..");
        //await _connection.StopAsync(cancellationToken);
        await _connection.DisposeAsync();
    }

    public async Task<string> EnvironmentUpdate(UpdateRequest updateRequest)
    {
        return await _mapper.UpdateService(updateRequest);
    }

    public async Task<ServiceResponse> Execute(ServiceRequest request)
    {
        return await _mapper.ExecuteService(request);
    }

    public async Task<Result<SessionData>> Start(SessionData session, CancellationToken token = default)
    {
        try
        {
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
            return response != null ? Result.Ok(response) : Result.Fail("Session data null");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error comunicationg with the server. {ex.StackTrace}");
        }
    }

    public async Task<Result<SessionData>> Next(string sessionId, object response, CancellationToken token = default)
    {
        try
        {
            var rawBin = response != null ? BinarySerialization.Serialize(response) : null;
            var result = await _connection.InvokeAsync<SessionData>("Next", sessionId, rawBin, token);
            return result != null ? Result.Ok(result) : Result.Fail("Session data null");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
        
    }

    private async Task Closed(Exception? error)
    {
        var errorMsg = error == null ? string.Empty : error.Message + error.StackTrace;

        _logger.Information($"Connection closed with the master node. \n{errorMsg}");
    }
}