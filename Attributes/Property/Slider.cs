using System;

namespace WilsonPluginInterface.Attributes.Property;

[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Slider : Gui
{
    public Slider(string title, string description = "", double min = 0.1f, double max = 1.0f, double step = 0.1)
    {
        Max = max;
        Min = min;
        Step = step;
        Title = title;
        Description = description;
    }

    public double Max { get; set; }

    public double Min { get; set; }

    public double Step { get; set; }
}