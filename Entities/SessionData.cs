using System.Collections.Generic;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Utility;                            

namespace WilsonEvoModuleLibrary.Entities;

public class SessionData
{
    public int ProcessId { get; set; }
    public int ProcessVersion { get; set; }
    public string SessionId { get; set; }
    public string CustomerId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public bool Test { get; set; }
   
    public string ChannelType { get; set; }
    public string Output { get; set; }
    public JObject Request { get; set; }
    public JObject ResponseByte { get; set; }
    public string CurrentNodeId { get; set; }
    public string CurrentShortUrl { get; set; }
    public string CurrentOutput { get; set; }
    public string CurrentPoolId { get; set; }
    public bool WaitingCallback { get; set; } = false;
    public bool ContinueExecution { get; set; } = true;
    public bool IsFaulted { get; set; } = false;
    public bool IsEndedCorrectly { get; set; } = false;
    public string Exception { get; set; } = string.Empty;
    public Dictionary<string, object> VarData { get; set; } = new();
    public Dictionary<string, int> OperationTracker { get; set; } = new();

   
    //public void SaveResponse<T>(T data)
    //{
    //    ResponseByte = BinarySerialization.Serialize(data);
    //}

    //public T? GetResponse<T>() where T : class
    //{
    //    return ResponseByte is null ? null : BinarySerialization.Deserialize<T>(ResponseByte);
    //}
}