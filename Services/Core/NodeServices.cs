using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core
{
    public abstract class NodeServices<TN, TC> : IExecutionService, INodeServices<TN, TC>
        where TN : class where TC : class
    {
        public Task Execute(in object nodeData, ref SessionData data, ref string output)
        {
            return Execute((TN)nodeData, ref data, ref output);
        }

        public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}