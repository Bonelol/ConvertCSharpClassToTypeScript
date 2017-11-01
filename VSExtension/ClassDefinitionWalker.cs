using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VSExtension
{
    public class ClassDefinitionWalker : MetaDataWalker<ClassDefinitionCollection>
    {
        public ICollection<ClassDeclarationSyntax> Classes { get; set; } = new List<ClassDeclarationSyntax>();

        public ClassDefinitionCache Cache { get; }

        public ClassDefinitionWalker(ClassDefinitionCache cache)
        {
            Cache = cache;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var baseName = node.Identifier.Text;
            var reference = new ReferenceCollection();
            var classDefinition = new ClassDefinition { TypeName = baseName };
            var propertyDeclarationWalker = new PropertyDeclarationWalker(Cache, reference);
            propertyDeclarationWalker.Visit(node);

            classDefinition.Properties = propertyDeclarationWalker.Collection;
            classDefinition.References = reference;

            this.Collection.Add(classDefinition);
        }

    }
}
