using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpFileParserTools
{
    public class PropertyDeclarationVisitor : MetaDataVisitor<PropertyDefinitionCollection>
    {
        public ClassDefinitionCache Cache { get; }
        public ReferenceCollection Reference { get; }

        public PropertyDeclarationVisitor(ClassDefinitionCache cache, ReferenceCollection reference)
        {
            Cache = cache;
            Reference = reference;
        }

        public override PropertyDefinitionCollection VisitClass_member_declaration(CSharpParser.Class_member_declarationContext context)
        {
            var memberDeclaration = context.common_member_declaration()?.typed_member_declaration();

            if (memberDeclaration == null)
                return this.Collection;

            var modifiers = context.all_member_modifiers()?.all_member_modifier();

            if (modifiers == null || modifiers.All(modifier => modifier.Start.Text != "public"))
                return this.Collection;

            var property = memberDeclaration.property_declaration();

            if (property == null)
                return this.Collection;

            var name = property.member_name().Start.Text;
            var type = memberDeclaration.type();
            var argVisitor = new TypeArgumentListVisitor(Cache, Reference);
            var typeArgs = argVisitor.Visit(type);
            var classDefinition = new ClassDefinition { BaseName = type.Start.Text, GenericTypeArgs = typeArgs };
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

            this.Collection.Add(new PropertyDefinition { Name = name, Type = classDefinition });

            return this.Collection;
        }
    }
}
