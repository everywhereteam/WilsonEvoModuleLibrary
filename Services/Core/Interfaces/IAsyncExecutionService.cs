using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

public interface IAsyncExecutionService
{
    Task Execute(in object nodeData, ref SessionData data, ref string output);
    Task ExecuteCallback(in object nodeData, ref SessionData session, ref string output);
}