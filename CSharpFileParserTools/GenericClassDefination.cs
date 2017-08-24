using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpFileParserTools
{
    public class GenericClassDefinition : ClassDefinition
    {
        public Dictionary<string, ClassDefinition> GenericTypeArgs { get; set; }
    }
}
