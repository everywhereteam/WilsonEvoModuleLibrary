using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces
{
    public interface IEnvironmentDeploy
    {
        Task HandleDeploy(List<BaseTask> nodes);
    }
}
