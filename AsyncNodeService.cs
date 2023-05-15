using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary
{
    public abstract class AsyncNodeService<TN> : IAsyncExecutionService, IAsyncNodeService<TN> where TN : INode
    {
        public Task Execute(in INode nodeData, ref SessionData sessionDataData, ref string output)
        {
            return Execute((TN) nodeData, ref sessionDataData, ref output);
        }

        public Task ExecuteCallback(in INode nodeData, ref SessionData sessionDataData, ref string output)
        {
            return ExecuteCallback((TN) nodeData, ref sessionDataData, ref output);
        }

        public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);

        public abstract Task ExecuteCallback(in TN nodeData, ref SessionData sessionDataData, ref string output);
    }
}