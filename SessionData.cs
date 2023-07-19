using System;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WilsonEvoModuleLibrary
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
        public byte[] Request { get; set; }
        public byte[] Response { get; set; }
        public string CurrentNodeId { get; set; }
        public string CurrentShortUrl { get; set; }
        public string CurrentOutput { get; set; }
        public string CurrentPoolId { get; set; }
        public bool WaitingCallback { get; set; } = false;
        public bool ContinueExecution { get; set; } = true;
        public Dictionary<string, object> VarData { get; set; } = new Dictionary<string, object>();

        public byte[] Serialize<T>(T data)
        {
            using var ms = new MemoryStream();
            using var writer = new BsonDataWriter(ms);
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            return ms.ToArray();
        }

        public T? Deserialize<T>(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BsonDataReader(ms);
            var serializer = new JsonSerializer();
            serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            return serializer.Deserialize<T>(reader);
        }
    }
}