using System.Threading.Tasks;

namespace WilsonPluginModels.Interfaces
{
    public interface IExecutionService
    {
        Task Execute(in INode nodeData, ref SessionData data, ref string output);
    }
}