using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InputCode : Gui
{
    public InputCode(string title, string description = "", InputCodeType codeType = InputCodeType.Any)
    {
        Title = title;
        Description = description;
        CodeType = codeType;
    }

    public InputCodeType CodeType { get; set; }
}