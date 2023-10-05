using System;               
using System.Threading;
using System.Threading.Tasks; 
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
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private ModelsConfiguration _configuration;
        private readonly ILogger _logger;



        public ModuleClient(ILogger logger,ModelsConfiguration configuration, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hubConnectionBuilder = hubConnectionBuilder;
            _mapper = mapper;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;

            _connection = _hubConnectionBuilder.WithAutomaticReconnect(new CustomRetryPolicy()).Build();

            _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);

            _connection.Closed += Closed;

            //_hostApplicationLifetime.ApplicationStarted.Register(() => Connect());
        }

        public async Task Log(LogLevel logLevel, EventId eventId, string? state = null, string? sessionId = null, Exception? exception = null, CancellationToken token = default)
        {
            await _connection.InvokeAsync("Log", logLevel, eventId, state, sessionId, exception, token);
        }

        public async Task<T?> Start<T>(object channel, string shortUrl, SessionData session = null, CancellationToken token = default) where T : class
        {
            session ??= new SessionData();
            session.ChannelType = channel.GetType().FullName ?? string.Empty;
            session.CurrentShortUrl = shortUrl;
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
            return response.GetResponse<T>();
        }

        public async Task<T?> Next<T>(string sessionId, object response, CancellationToken token = default) where T : class
        {
            var result = await _connection.InvokeAsync<SessionData>("Next", sessionId, response, token);
            return result.GetResponse<T>();
        }

        public async Task<ServiceResponse> Execute(ServiceRequest request)
        {
            return await _mapper.ExecuteService(request);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);
                    if (_connection.State == HubConnectionState.Connected)
                    {
                        await _connection.InvokeAsync("RegisterServicesDAQ", _configuration,
                            cancellationToken: cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    await _connection.StopAsync(cancellationToken);
                }

            }

        }

        private async Task Connect()
        {

        }

        private async Task Closed(Exception? error)
        {
            var errorMsg = error == null ? string.Empty : error.Message + error.StackTrace;

            _logger.LogInformation($"Connection closed with the master node. \n{errorMsg}");

        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("[Wilson] Closing Connection..");
            //await _connection.StopAsync(cancellationToken);
            await _connection.DisposeAsync();
        }


    }
}
