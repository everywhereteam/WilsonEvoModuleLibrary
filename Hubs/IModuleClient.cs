using System.Threading.Tasks;
using FluentResults;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Hubs
{
    public interface IModuleClient
    {
        public Task<Result<ServiceResponse>> Execute(ServiceRequest request);  
    } 
}
