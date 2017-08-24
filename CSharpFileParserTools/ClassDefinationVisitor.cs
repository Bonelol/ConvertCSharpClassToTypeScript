using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;

namespace CSharpFileParserTools
{
    public class ClassDefinitionVisitor : MetaDataVisitor<ClassDefinitionCollection>
    {
        public ClassDefinitionCache Cache { get; }

        public ClassDefinitionVisitor(ClassDefinitionCache cache)
        {
            Cache = cache;
        }

        public override ClassDefinitionCollection VisitClass_definition(CSharpParser.Class_definitionContext context)
        {
            var reference = new ReferenceCollection();
            var baseName = ((ParserRuleContext)context.children[1]).Start.Text;
            var argsVisitor = new TypeArgumentListVisitor(Cache, reference);
            var classDefinition = new ClassDefinition { BaseName = baseName };
            var typeArgs = context.type_parameter_list();

            if (typeArgs != null)
            {
                var args = argsVisitor.Visit(typeArgs);
                classDefinition.GenericTypeArgs = args;
            }

            var className = classDefinition.Name;

            if (!Cache.ContainsKey(className))
            {
                Cache.Add(className, classDefinition);
            }
            else
            {
                classDefinition = Cache[className];
            }

            var visitor = new PropertyDeclarationVisitor(Cache, reference);
            var properties = visitor.Visit(context);
            classDefinition.Properties = properties;
            classDefinition.References = reference;

            this.Collection.Add(classDefinition);
            return this.Collection;
        }
    }
}
