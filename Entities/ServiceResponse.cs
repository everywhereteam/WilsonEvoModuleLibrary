using WilsonEvoCoreLibrary.Core.Models;

namespace WilsonEvoModuleLibrary.Entities;

public class ServiceResponse
{
    public ProcessSession Session { get; set; }
    public uint ExecutionTime { get; set; }
    public float ModuleUsage { get; set; }
}