using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSExtension
{
    public class ClassDefinition
    {
        public string TypeName { get; set; }
        public string TypeScriptBaseName => _typeNameMappings.ContainsKey(TypeName) ? _typeNameMappings[TypeName] : TypeName;
        public string Name => this.IsGeneric ? $"{TypeName}<{string.Join(",", this.TypeParameters.Select(a => a.Name))}>" : TypeName;
        public string TypeScriptName => this.IsGeneric ? $"{TypeScriptBaseName}<{string.Join(",", this.TypeParameters.Select(a => a.Name))}>" : TypeScriptBaseName;
        public bool IsGeneric => this.TypeParameters != null && this.TypeParameters.Count > 0;
        public ICollection<ClassDefinition> TypeParameters { get; set; }
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
