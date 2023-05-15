﻿using System.Threading.Tasks;

namespace WilsonPluginModels.Interfaces
{
    public interface IAsyncExecutionService
    {
        Task Execute(in INode nodeData, ref SessionData data, ref string output);
        Task ExecuteCallback(in INode nodeData, ref SessionData sessionDataData, ref string output);
    }
}