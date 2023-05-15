using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Interfaces
{
    public interface IAsyncNodeServices<TN, TC> where TN : INode where TC : class
    {
        Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}