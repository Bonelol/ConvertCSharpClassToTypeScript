using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConvertCSharpClassToTypeScript
{
    public class MetaDataWalker<T> : CSharpSyntaxWalker where T : new()
    {
        public T Collection { get; set; } = new T();

        protected static string GetTypeName(TypeSyntax type)
        {
            var name = "";

            switch (type)
            {
                case PredefinedTypeSyntax predefined:
                    name = predefined.Keyword.Text;
                    break;
                case NullableTypeSyntax nullable:
                    name = GetTypeName(nullable.ElementType);
                    break;
                case SimpleNameSyntax simple:
                    name = simple.Identifier.Text;
                    break;
            }

            return name;
        }
    }
}
