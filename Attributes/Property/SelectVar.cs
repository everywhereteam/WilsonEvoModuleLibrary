using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SelectVar : Gui
{
    public SelectVar(Type type, string title, string description = "")
    {
        TypeFilter = type.FullName ?? throw new InvalidOperationException();
        Title = title;
        Description = description;
    }

    public string TypeFilter { get; set; }
}