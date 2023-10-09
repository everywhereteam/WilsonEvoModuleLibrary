using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core
{
    public abstract class NodeService<TN> : IExecutionService, INodeService<TN> where TN : BaseTask
    {
        public Task Execute(in object nodeData, ref SessionData data, ref string output)
        {
            return Execute((TN)nodeData, ref data, ref output);
        }

        public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}