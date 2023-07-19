using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace WilsonEvoModuleLibrary
{
    public class SessionData
    {
        public SessionData()
        {
        }

        public SessionData(string sessionId, int processCode, JObject request = null)
        {
            Request = request;
            SessionId = sessionId;
            ProcessVersion = processCode;
        }

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
        public JObject Request { get; set; }
        public JObject Response { get; set; }
        public string CurrentNodeId { get; set; }
        public string CurrentShortUrl { get; set; }
        public string CurrentOutput { get; set; }
        public string CurrentPoolId { get; set; }
        public bool WaitingCallback { get; set; } = false;
        public bool ContinueExecution { get; set; } = true;
        public Dictionary<string, object> VarData { get; set; } = new Dictionary<string, object>();
    }
}