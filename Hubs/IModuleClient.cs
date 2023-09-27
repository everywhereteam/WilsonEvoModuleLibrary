using System.Threading.Tasks;
using FluentResults;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Hubs
{
    public interface IModuleClient
    {
        public Task<ServiceResponse> Execute(ServiceRequest request);  
    } 
}
