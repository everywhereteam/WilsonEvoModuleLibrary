﻿using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Interfaces
{
    public interface IAsyncNodeService<TN> where TN : INode
    {
        Task Execute(in TN nodeData, ref SessionData data, ref string output);
    }
}