﻿using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Models;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class AsyncNodeServices<TTask, TConfiguration> : IAsynchronousTaskExecutor where TTask : ITask where TConfiguration : IModuleConfiguration
{
    public abstract Task Execute(TTask nodeData, ProcessSession session, TConfiguration configuration);

    public abstract Task ExecuteCallback(TTask nodeData, ProcessSession session, TConfiguration configuration);
}