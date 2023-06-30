using BlazorDynamicFormGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Hubs
{
    public interface IModuleClient
    {
        public Task<ServiceResponse?> Execute(ServiceRequest request);
        public Task<Dictionary<string, ModuleNodeDefinition>> ModuleConfiguration();
    }
}
