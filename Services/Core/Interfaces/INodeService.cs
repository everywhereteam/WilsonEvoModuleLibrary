using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Models;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

internal interface INodeService<TN> where TN : ITask
{
    Task Execute(TN nodeData, ProcessSession session);
}