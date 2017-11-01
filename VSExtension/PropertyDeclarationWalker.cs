using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VSExtension
{
    public class PropertyDeclarationWalker : MetaDataWalker<PropertyDefinitionCollection>
    {
        public ClassDefinitionCache Cache { get; }
        public ReferenceCollection Reference { get; }

        public PropertyDeclarationWalker(ClassDefinitionCache cache, ReferenceCollection reference)
        {
            Cache = cache;
            Reference = reference;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var propertyName = node.Identifier.Text;
            var argVisitor = new TypeArgumentListWalker(Cache, Reference);
            argVisitor.Visit(node.Type);

            var name = GetTypeName(node.Type);
            var classDefinition = new ClassDefinition { TypeName = name, TypeParameters = argVisitor.Collection };
            var className = classDefinition.Name;

            if (!Cache.ContainsKey(className))
            {
                Cache.Add(className, classDefinition);
            }
            else
            {
                classDefinition = Cache[className];
            }

            Reference.Add(className);

            this.Collection.Add(new PropertyDefinition { Name = propertyName, Type = classDefinition });
        }
    }
}
