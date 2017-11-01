using System.Collections.Generic;

namespace VSExtension
{
    public class GenericClassDefinition : ClassDefinition
    {
        public Dictionary<string, ClassDefinition> GenericTypeArgs { get; set; }
    }
}
