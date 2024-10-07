using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

internal interface IExecutionService
{
    Task Execute(object nodeData, SessionData data);
}