using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

public interface IExecutionService
{
    Task Execute(in object nodeData, ref SessionData session, ref string output);
}