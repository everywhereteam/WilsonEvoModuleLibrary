using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Hubs;

public interface IModuleClient
{
    public Task<ServiceResponse> Execute(ServiceRequest request);
}