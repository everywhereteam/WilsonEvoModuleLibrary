using System;

namespace WilsonPluginInterface.Attributes.Property;

[Serializable]
public enum InputCodeType
{
    Any,
    CSharp,
    Javascript,
    Python,
    Sql
}