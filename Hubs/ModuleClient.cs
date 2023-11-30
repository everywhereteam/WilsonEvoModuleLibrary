﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;        
using Microsoft.Extensions.Hosting;
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

    public ModuleClient(ILogger logger, ModelsConfiguration configuration,
        IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;

        _connection = hubConnectionBuilder.WithAutomaticReconnect(new CustomRetryPolicy()).Build();
        _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);
        _connection.Closed += Closed;
        _connection.ServerTimeout = TimeSpan.FromSeconds(25);
        _connection.HandshakeTimeout = TimeSpan.FromSeconds(25);
       // _connection.KeepAliveInterval = TimeSpan.FromSeconds(10);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
            try
            {
                await _connection.StartAsync(cancellationToken);
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("RegisterServices", _configuration,
                        cancellationToken);

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

    public async Task<ServiceResponse> Execute(ServiceRequest request)
    {
        return await _mapper.ExecuteService(request);
    }

    public async Task<Result<T?>> Start<T>(object channel, string shortUrl, SessionData? session = null,
        CancellationToken token = default) where T : class
    {
        session ??= new SessionData();
        session.ChannelType = channel.GetType().Name ?? string.Empty;
        session.CurrentShortUrl = shortUrl;
        try
        {
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
           
            //return Result.Ok(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(response.Response)));
            return Result.Ok(BinarySerialization.Deserialize<T>(response.Response));
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error comunicationg with the server. {ex.StackTrace}");
        }
    }

    public async Task<T?> Next<T>(string sessionId, object response, CancellationToken token = default)
        where T : class
    {
        var result = await _connection.InvokeAsync<SessionData>("Next", sessionId, response, token);
        return BinarySerialization.Deserialize<T>(result.Response); //.GetResponse<T>();
    }

    private async Task Closed(Exception? error)
    {
        var errorMsg = error == null ? string.Empty : error.Message + error.StackTrace;

        _logger.Information($"Connection closed with the master node. \n{errorMsg}");
    }
}