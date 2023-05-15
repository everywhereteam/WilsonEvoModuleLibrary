using System;

namespace WilsonPluginInterface.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DictionaryList : Gui
{
    public DictionaryList(string title, string description = "")
    {
        Title = title;
        Description = description;
    }

    public Gui Child { get; set; }
}