using System;

namespace WilsonPluginInterface.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SelectNode<T> : Gui
{
    public SelectNode(string title, string description = "")
    {
        TypeFilter = typeof(T).FullName;
        Title = title;
        Description = description;
    }

    public string TypeFilter { get; set; }
}