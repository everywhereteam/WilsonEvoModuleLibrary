using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities
{
    public class SessionData
    {

        public int ProcessId { get; set; }
        public int ProcessVersion { get; set; }
        public string SessionId { get; set; }
        public string CustomerId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Test { get; set; }
        public Dictionary<string, string> ChannelData { get; set; } = new Dictionary<string, string>();
        public string ChannelType { get; set; }
        public string Output { get; set; }
        public dynamic Request { get; set; }
        public dynamic Response { get; set; }
        public string CurrentNodeId { get; set; }
        public string CurrentShortUrl { get; set; }
        public string CurrentOutput { get; set; }
        public string CurrentPoolId { get; set; }
        public bool WaitingCallback { get; set; } = false;
        public bool ContinueExecution { get; set; } = true;
        public bool IsFaulted { get; set; } = false;
        public string Exception { get; set; } = string.Empty;
        public Dictionary<string, object> VarData { get; set; } = new Dictionary<string, object>();


    }
}