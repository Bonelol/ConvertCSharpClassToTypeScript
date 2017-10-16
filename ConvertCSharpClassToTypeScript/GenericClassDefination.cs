using System.Collections.Generic;

namespace ConvertCSharpClassToTypeScript
{
    public class GenericClassDefinition : ClassDefinition
    {
        public Dictionary<string, ClassDefinition> GenericTypeArgs { get; set; }
    }
}
