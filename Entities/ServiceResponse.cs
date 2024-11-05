using WilsonEvoCoreLibrary.Core.Models;

namespace WilsonEvoModuleLibrary.Entities;

public class ServiceResponse
{
    public ProcessSession Session { get; set; }
    // public List<ProcessChatHistory> ChatHistory { get; set; } = new List<ProcessChatHistory>();
    //  public List<Log> Logs { get; set; }

    //public bool IsFailed { get; set; } = false;
    // public string? ExceptionMessage { get; set; }


    //Metrics
    public uint ExecutionTime { get; set; }
    public float ModuleUsage { get; set; }
}