using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MultipleSelect : Gui
{
    public MultipleSelect(string title, string description = "", params string[] options)
    {
        Options = options;
        Title = title;
        Description = description;
    }

    public string[] Options { get; set; }
}