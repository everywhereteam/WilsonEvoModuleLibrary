using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

internal interface IAsyncExecutionService
{
    Task Execute(in object nodeData, ref SessionData sessionData, ref string output);
    Task ExecuteCallback(in object nodeData, ref SessionData sessionData, ref string output);
}