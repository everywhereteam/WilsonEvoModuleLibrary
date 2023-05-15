using System;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceProviderConfigurationAttribute : Attribute
{
    public ServiceProviderConfigurationAttribute(string name)
    {
        ServiceName = name;
    }

    public string ServiceName { get; set; }
}