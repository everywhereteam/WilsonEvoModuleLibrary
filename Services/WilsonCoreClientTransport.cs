using System;     
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WilsonEvoModuleLibrary.Services
{
    internal class WilsonCoreClientTransport : IHostedService
    {
        private readonly IHubConnectionBuilder _hubConnectionBuilder;
        private HubConnection? _connection;
        private readonly NodeServiceMapper _mapper;
        private readonly ILogger _logger;

        public WilsonCoreClientTransport(ILogger<WilsonCoreClientTransport> logger, IHubConnectionBuilder hubConnectionBuilder, NodeServiceMapper mapper)
        {
            _hubConnectionBuilder = hubConnectionBuilder;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task GetModuleProjectVersionConfiguration<T>(int projectVersion, CancellationToken token = default)
        {
            await _connection.InvokeAsync<T>("GetModuleProjectVersionConfiguration", projectVersion, token);
        }

        private async Task<ServiceResponse> Execute(ServiceRequest request)
        {
            return await _mapper.ExecuteService(request);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _hubConnectionBuilder
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
                .Build();

            _connection.On<ServiceRequest, ServiceResponse>("Execute", Execute);

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
