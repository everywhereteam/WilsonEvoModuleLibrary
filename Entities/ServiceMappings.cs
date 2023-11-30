using System;
using System.Collections.Concurrent;

namespace WilsonEvoModuleLibrary.Entities;

public class ServiceMappings
{
    public ConcurrentDictionary<MapPath, Type> ServiceMap { get; set; } = new();
}