using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces
{
    internal interface IAsyncNodeService<TN> where TN : BaseTask
    {
        Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}