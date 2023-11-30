using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;

public class ServiceRequest
{
    public ServiceRequest()
    {
    }

    public ServiceRequest(SessionData sessionData, byte[] nodeData, string type)
    {
        SessionData = sessionData;
        NodeData = nodeData;
        Type = type;
    }

    
    public SessionData SessionData { get; set; }
    public byte[] NodeData { get; set; }
    public string Type { get; set; }
}