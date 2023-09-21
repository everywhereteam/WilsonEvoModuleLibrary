using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces
{
    internal interface IExecutionService
    {
        Task Execute(in object nodeData, ref SessionData data, ref string output);
    }
}