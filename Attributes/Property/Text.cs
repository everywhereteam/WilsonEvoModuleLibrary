using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class Text : Gui
{
    public Text(string text)
    {
        DisplayText = text;
    }

    public string DisplayText { get; set; }
}