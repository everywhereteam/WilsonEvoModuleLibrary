using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Models;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class TaskExecutorBase<TTask, TConfiguration> : ISynchronousTaskExecutor where TTask : ITask where TConfiguration : IModuleConfiguration
{
    public abstract Task Execute(TTask nodeData, ProcessSession session, TConfiguration configuration);
}