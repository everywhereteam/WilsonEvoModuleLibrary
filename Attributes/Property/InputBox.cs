using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InputBox : Gui
{
    public InputBox(string title, string description = "")
    {
        Title = title;
        Description = description;
    }
}