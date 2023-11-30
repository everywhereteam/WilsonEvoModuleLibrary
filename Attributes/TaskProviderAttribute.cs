using System;
using System.Runtime.Serialization;
using BlazorDynamicFormGenerator;
using FluentResults;
using MessagePack;
using MessagePack.Formatters;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskProviderAttribute : Attribute
{                                                                  
    public ModuleNodeDefinition Definition { get; set; }
}