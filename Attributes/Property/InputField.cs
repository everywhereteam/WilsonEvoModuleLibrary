using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InputField : Gui
{
    public InputField(string title, string description = "", string regex = "")
    {
        Regex = regex;
        Title = title;
        Description = description;
    }

    public string Regex { get; set; }
}