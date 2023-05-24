using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Attributes
{
    public class ModuleNodeDefinition
    {
        public List<ModuleNodePropertyDefinition> PropertyDefinitions { get; set; } = new();


    }

    public class ModuleNodePropertyDefinition
    {
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public DataType? DataType { get; set; }
        public string? CustomDataType { get; set; }

        public string[] SelectionStrings { get; set; }
    }

    public static class ModuleNodePropertyDefinitionExtensions
    {
        public static ModuleNodeDefinition GetDefinition<T>(this T obj)
        {
            var definition = new ModuleNodeDefinition();
            var proList = typeof(T).GetProperties();
            foreach (var prp in proList)
            {
                var attrList = (DataTypeAttribute)prp.GetCustomAttributes(typeof(DataTypeAttribute), false).First();
                var displayLabel = (DisplayAttribute)prp.GetCustomAttributes(typeof(DisplayAttribute), false).First();
                // Get the initial property value
                var servicePropDef = new ModuleNodePropertyDefinition()
                {
                    Name = prp.Name,
                    DisplayName = displayLabel?.Name,
                    DataType = attrList?.DataType,
                    CustomDataType = attrList?.CustomDataType
                };
              
                definition.PropertyDefinitions.Add(servicePropDef);

            }
            return definition;
        }
    }

    
}
