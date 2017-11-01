using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VSExtension
{
    public class TypeArgumentListWalker : MetaDataWalker<ClassDefinitionCollection>
    {
        public ClassDefinitionCache Cache { get; }
        public ReferenceCollection Reference { get; }

        public TypeArgumentListWalker(ClassDefinitionCache cache, ReferenceCollection reference)
        {
            Cache = cache;
            Reference = reference;
        }

        public override void VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            foreach (var argument in node.Arguments)
            {
                var name = GetTypeName(argument);
                var argVisitor = new TypeArgumentListWalker(Cache, Reference);
                argVisitor.Visit(argument);

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
                this.Collection.Add(classDefinition);
            }
        }
    }
}
