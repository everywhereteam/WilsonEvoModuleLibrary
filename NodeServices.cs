using System.Threading.Tasks;
using WilsonPluginModels.Interfaces;

namespace WilsonPluginModels
{
    public abstract class NodeServices<TN, TC> : IExecutionService, INodeServices<TN, TC>
        where TN : INode where TC : class
    {
        public Task Execute(in INode nodeData, ref SessionData data, ref string output)
        {
            return Execute((TN) nodeData, ref data, ref output);
        }

        public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}