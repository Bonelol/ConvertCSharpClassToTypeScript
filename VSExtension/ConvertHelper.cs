using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;

namespace VSExtension
{
    public class ConvertHelper
    {
        public ClassDefinitionCache Cache { get; }

        public ConvertHelper(ClassDefinitionCache cache = null)
        {
            Cache = cache ?? new ClassDefinitionCache();
        }

        public List<ClassDefinition> ParseFile(string filePath)
        {
            var stream = File.OpenRead(filePath);
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var tree = CSharpSyntaxTree.ParseText(content);

            tree.TryGetRoot(out var root);

            var visitor = new ClassDefinitionWalker(new ClassDefinitionCache());
            visitor.Visit(root);

            return visitor.Collection;
        }
    }
}
