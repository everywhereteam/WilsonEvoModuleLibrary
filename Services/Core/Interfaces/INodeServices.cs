using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

internal interface INodeServices<TN, TC> where TN : BaseTask where TC : class
{
    Task Execute(in TN nodeData, ref SessionData data, ref string output);
}