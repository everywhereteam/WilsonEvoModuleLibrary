using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Network
{
    public interface IModuleChannelClient
    {
        public Task<ServiceResponse> Execute(ServiceRequest request, CancellationToken token);
        public Task<string> EnvironmentUpdate(UpdateRequest updateRequest, CancellationToken token);
        public Task<Result<SessionData>> Start(string shortUrl, string? channelName = "", SessionData session = null, CancellationToken token = default);
        public Task<Result<SessionData>> Next(string sessionId, object? requestData, CancellationToken token = default);
        public Task RegisterServices(ModelsConfiguration configuration, CancellationToken token = default);
    }

    public interface IModuleChannelServer
    {
        public Task<ServiceResponse> Execute(string connectionId, ServiceRequest request, CancellationToken token);
        public Task<string> EnvironmentUpdate(string connectionId, UpdateRequest updateRequest, CancellationToken token);
    }
}
