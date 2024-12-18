﻿using WilsonEvoCoreLibrary.Core.Models;

namespace WilsonEvoModuleLibrary.Entities;

public class ServiceRequest
{
    public ServiceRequest()
    {
    }

    public ServiceRequest(ProcessSession sessionData, byte[] nodeData, string type)
    {
        SessionData = sessionData;
        NodeData = nodeData;
        Type = type;
    }


    public ProcessSession SessionData { get; set; }
    public byte[]? ModuleConfiguration { get; set; }
    public byte[] NodeData { get; set; }
    public string Type { get; set; }
}