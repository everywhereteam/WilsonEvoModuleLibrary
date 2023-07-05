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
        public Task Run(ServiceRequest request);
        public Task ModuleConfiguration();
    }

    public interface IModuleServer
    {
        public Task Run(ServiceResponse response);
        public Task ModuleConfiguration(Modelsconfiguration configuration);
    }
}
