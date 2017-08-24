using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertCSharpClassToTypeScript
{
    public class PropertyDefinition
    {
        public string Name { get; set; }
        public ClassDefinition Type { get; set; }
    }
}
