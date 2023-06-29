using System;
using System.Data.Common;
using System.Net.Http;
using System.Text;
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

        public ModuleClient(ILogger<ModuleClient> logger, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper)
        {
            _hubConnectionBuilder = hubConnectionBuilder;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Log(LogLevel logLevel, EventId eventId, string? state = null, string? sessionId = null, Exception? exception = null, CancellationToken token = default)
        {
            await _connection.InvokeAsync("Log", logLevel, eventId, state, sessionId, exception, token);
        }

        public async Task<R> Start<R>(object channel, SessionData session)
        {
            session.ChannelType = channel.GetType().AssemblyQualifiedName ?? string.Empty;
            var response = await _connection.InvokeAsync<SessionData>("Start", session);
            return (R)response.Response;
        }

        public async Task<SessionData> Next(string sessionId, object response, CancellationToken token = default)
        {
            return await _connection.InvokeAsync<SessionData>("Next", sessionId, response, token);
        }

        public async Task<ServiceResponse> Execute(ServiceRequest request, CancellationToken token)
        {
            return await _mapper.ExecuteService(request);
        }

        public Task<byte[]> ModuleConfiguration()
        {

            //TODO
            return null;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _hubConnectionBuilder.WithAutomaticReconnect().Build();

            _connection.On<ServiceRequest, CancellationToken, ServiceResponse>("Execute", Execute);
            _connection.On<CancellationToken, byte[]>("ModuleConfiguration", ModuleConfiguration);
            _connection.Closed += Reconnect;

            await _connection.StartAsync(cancellationToken);

        }

        private async Task Reconnect(Exception? error)
        {
            var errorMsg = error == null ? string.Empty : error.Message;
            _logger.LogWarning("[Wilson] Reconnecting.. {errorMsg}", errorMsg);
            await Task.Delay(new Random().Next(0, 5) * 1000);
            if (_connection != null)
                await _connection.StartAsync();
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("[Wilson] Closing Connection..");
            if (_connection != null)
                await _connection.DisposeAsync();
        }


    }
}
