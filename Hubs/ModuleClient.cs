using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services;

namespace WilsonEvoModuleLibrary.Hubs
{
    public class CustomRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return TimeSpan.FromSeconds(5);
        }
    }
    public sealed class ModuleClient : IHostedService, IModuleClient
    {
        private readonly IHubConnectionBuilder _hubConnectionBuilder;
        private HubConnection _connection;
        private readonly NodeServiceMapper _mapper;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ModelsConfiguration _configuration;

        public ModuleClient(ModelsConfiguration configuration, ILogger<ModuleClient> logger, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hubConnectionBuilder = hubConnectionBuilder;
            _mapper = mapper;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;


            _connection = _hubConnectionBuilder.WithAutomaticReconnect(new CustomRetryPolicy() ).Build();

            _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);

            _connection.Closed += Reconnect;
            _hostApplicationLifetime.ApplicationStarted.Register(() => Connect());
        }

        public async Task Log(LogLevel logLevel, EventId eventId, string? state = null, string? sessionId = null, Exception? exception = null, CancellationToken token = default)
        {
            await _connection.InvokeAsync("Log", logLevel, eventId, state, sessionId, exception, token);
        }

        public async Task<dynamic> Start(object channel, string shortUrl, SessionData session = null, CancellationToken token = default)
        {
            session ??= new SessionData();
            session.ChannelType = channel.GetType().AssemblyQualifiedName ?? string.Empty;
            session.CurrentShortUrl = shortUrl;
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
            return response.Response;
        }

        public async Task<dynamic> Next(string sessionId, object response, CancellationToken token = default)
        {
            var result = await _connection.InvokeAsync<SessionData>("Next", sessionId, response, token);
            return result.Response;
        }

        public async Task<ServiceResponse> Execute(ServiceRequest request)
        {
            return await _mapper.ExecuteService(request);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {                           
        }

        private async Task Connect()
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("RegisterServices", _configuration);
            //int retryCount = 0;

            //while (true)
            //{
            //    try
            //    {

            //        break;
            //    }
            //    catch (Exception e)
            //    {

            //        retryCount++;
            //        Console.WriteLine($"Failed to start the connection with the server. Attempt number {retryCount}. \n{e.Message}\n{e.StackTrace}");
            //        await Task.Delay(TimeSpan.FromSeconds(10));
            //    }
            //}
        }

        private async Task Reconnect(Exception? error)
        {
            var errorMsg = error == null ? string.Empty : error.Message;
            Console.WriteLine($"Error occurred: {errorMsg}. Reconnecting...");
            //await Task.Delay(TimeSpan.FromSeconds(5));
       
            //try
            //{
            //    await _connection.StartAsync();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine($"Error when trying to reconnect: {e.Message}\n{e.StackTrace}");
            //}
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("[Wilson] Closing Connection..");
            if (_connection != null)
                await _connection.DisposeAsync();
        }


    }
}
