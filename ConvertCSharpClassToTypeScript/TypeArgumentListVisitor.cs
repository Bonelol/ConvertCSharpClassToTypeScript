using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertCSharpClassToTypeScript
{
    public class TypeArgumentListVisitor : MetaDataVisitor<ClassDefinitionCollection>
    {
        public ClassDefinitionCache Cache { get; }
        public ReferenceCollection Reference { get; }

        public TypeArgumentListVisitor(ClassDefinitionCache cache, ReferenceCollection reference)
        {
            Cache = cache;
            Reference = reference;
        }

        public override ClassDefinitionCollection VisitType_argument_list(CSharpParser.Type_argument_listContext context)
        {
            var types = context.type();
            foreach (var type in types)
            {
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
                this.Collection.Add(classDefinition);
            }

            return this.Collection;
        }
    }
}
