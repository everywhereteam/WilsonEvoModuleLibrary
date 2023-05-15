using System;

namespace WilsonPluginInterface.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceProviderConfigurationAttribute : Attribute
{
    public ServiceProviderConfigurationAttribute(string name)
    {
        ServiceName = name;
    }

    public string ServiceName { get; set; }
}