using System;
using System.Threading;
using System.Threading.Tasks;      
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WilsonEvoModuleLibrary.Services;

namespace WilsonEvoModuleLibrary.Hubs
{
    public sealed class ModuleClient : IHostedService, IModuleClient
    {
        private readonly IHubConnectionBuilder _hubConnectionBuilder;
        private HubConnection _connection;
        private readonly NodeServiceMapper _mapper;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public ModuleClient(ILogger<ModuleClient> logger, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper, IHostApplicationLifetime hostApplicationLifetime)
        {
            _hubConnectionBuilder = hubConnectionBuilder;
            _mapper = mapper;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;



            _connection = _hubConnectionBuilder.WithAutomaticReconnect().Build();

            _connection.On<ServiceRequest, ServiceResponse>(nameof(Execute), Execute);
            _connection.On(nameof(ModuleConfiguration), ModuleConfiguration);                                        

            _connection.Closed += Reconnect;    
            _hostApplicationLifetime.ApplicationStarted.Register(() => Connect());
        }

        public async Task Log(LogLevel logLevel, EventId eventId, string? state = null, string? sessionId = null, Exception? exception = null, CancellationToken token = default)
        {
            await _connection.InvokeAsync("Log", logLevel, eventId, state, sessionId, exception, token);
        }

        public async Task<R> Start<R>(object channel, string shortUrl, SessionData session = null, CancellationToken token = default)
        {
            session ??= new SessionData();
            session.ChannelType = channel.GetType().AssemblyQualifiedName ?? string.Empty;
            session.CurrentShortUrl = shortUrl;
            var response = await _connection.InvokeAsync<SessionData>("Start", session, token);
            return (R)response.Response;
        }

        public async Task<SessionData> Next(string sessionId, object response, CancellationToken token = default)
        {
            return await _connection.InvokeAsync<SessionData>("Next", sessionId, response, token);
        }

        public async Task<ServiceResponse> Execute(ServiceRequest request)
        {
            return await _mapper.ExecuteService(request);
        }

        public async Task<Modelsconfiguration> ModuleConfiguration()
        {
            var response = new Modelsconfiguration();
            response.Definitions = _mapper.GetDefinitions();
            return response;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

        }

        private async Task Connect()
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    await _connection.StartAsync();
                    break;
                }
                catch (Exception e)
                {

                    retryCount++;
                    Console.WriteLine($"Failed to start the connection with the server. Attempt number {retryCount}. ");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }

        private async Task Reconnect(Exception? error)
        {
            var errorMsg = error == null ? string.Empty : error.Message;
            Console.WriteLine($"Error occurred: {errorMsg}. Reconnecting...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (_connection.State == HubConnectionState.Connected || _connection.State == HubConnectionState.Connecting)
            {
                return;
            }
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when trying to reconnect: {e.Message}");
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("[Wilson] Closing Connection..");
            if (_connection != null)
                await _connection.DisposeAsync();
        }


    }
}
