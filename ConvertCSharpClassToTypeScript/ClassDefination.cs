using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertCSharpClassToTypeScript
{
    public class ClassDefinition
    {
        public string BaseName { get; set; }
        public string TypeScriptBaseName => _typeNameMappings.ContainsKey(BaseName) ? _typeNameMappings[BaseName] : BaseName;
        public string Name => this.IsGeneric ? $"{BaseName}<{string.Join(",", this.GenericTypeArgs.Select(a => a.Name))}>" : BaseName;
        public string TypeScriptName => this.IsGeneric ? $"{TypeScriptBaseName}<{string.Join(",", this.GenericTypeArgs.Select(a => a.Name))}>" : TypeScriptBaseName;
        public bool IsGeneric => this.GenericTypeArgs != null && this.GenericTypeArgs.Count > 0;
        public ICollection<ClassDefinition> GenericTypeArgs { get; set; }
        public ICollection<PropertyDefinition> Properties { get; set; }
        public ICollection<string> References { get; set; }

        private static readonly Dictionary<string, string> _typeNameMappings = new Dictionary<string, string>()
        {
            {"short", "number"},
            {"Short", "number"},
            {"int", "number"},
            {"Int16", "number"},
            {"Int32", "number"},
            {"double", "number"},
            {"Double", "number"},
            {"float", "number"},
            {"Float", "number"},

            {"bool", "boolean"},
            {"Boolean", "boolean"},
            {"DateTime", "Date"},

            {"IEnumerable", "Array"},
            {"ICollection", "Array"},
            {"IList", "Array"},
            {"List", "Array"},
            {"DbSet", "Array"},
            {"HashSet", "Array"},
        };
    }
}
