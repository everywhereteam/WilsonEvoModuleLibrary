﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Hubs
{
    public interface IModuleClient
    {
        public Task<ServiceResponse?> Execute(ServiceRequest request, CancellationToken token);
        public Task<byte[]> ModuleConfiguration(CancellationToken token);
    }
}