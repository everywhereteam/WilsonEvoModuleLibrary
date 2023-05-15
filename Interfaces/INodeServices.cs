using System.Threading.Tasks;

namespace WilsonPluginModels.Interfaces
{
    public interface INodeServices<TN, TC> where TN : INode where TC : class
    {
        Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}