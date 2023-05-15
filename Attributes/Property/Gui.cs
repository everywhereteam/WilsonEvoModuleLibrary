using System;

namespace WilsonEvoModuleLibrary.Attributes.Property;

public abstract class Gui : Attribute
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }
}