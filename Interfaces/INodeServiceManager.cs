using System.Threading;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Interfaces
{
    public interface INodeServiceManager
    {
        Task<SessionData> Next(string sessionId, object response, CancellationToken token);
        Task<SessionData> Start(SessionData session, CancellationToken token);
    }
}