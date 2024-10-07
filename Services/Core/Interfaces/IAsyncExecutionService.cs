using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

internal interface IAsyncExecutionService
{
    Task Execute(object nodeData, SessionData sessionData);
    Task ExecuteCallback(object nodeData, SessionData sessionData);
}