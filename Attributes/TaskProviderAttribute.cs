using System;
using BlazorDynamicForm.Entities;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskProviderAttribute : Attribute
{                                                                  
    public FormMap Definition { get; set; }
}