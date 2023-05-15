using System;

namespace WilsonPluginInterface.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Checkbox : Gui
{
    public Checkbox(string title, string description = "")
    {
        Title = title;
        Description = description;
    }
}