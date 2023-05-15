using System.Threading.Tasks;
using WilsonPluginModels.Interfaces;

namespace WilsonPluginModels
{
    public abstract class NodeService<TN> : IExecutionService, INodeService<TN> where TN : INode
    {
        public Task Execute(in INode nodeData, ref SessionData data, ref string output)
        {
            return Execute((TN) nodeData, ref data, ref output);
        }

        public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}