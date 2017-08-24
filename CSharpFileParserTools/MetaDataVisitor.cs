using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Tree;

namespace CSharpFileParserTools
{
    public class MetaDataVisitor<T> : CSharpParserBaseVisitor<T> where T : new()
    {
        public T Collection { get; set; } = new T();

        public override T Visit(IParseTree tree)
        {
            base.Visit(tree);
            return Collection;
        }
    }
}
