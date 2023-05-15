using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Interfaces
{
    public interface IExecutionService
    {
        Task Execute(in INode nodeData, ref SessionData data, ref string output);
    }
}