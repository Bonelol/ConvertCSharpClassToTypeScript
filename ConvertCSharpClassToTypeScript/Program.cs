using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Antlr4.Runtime;
using CSharpFileParserTools;

namespace ConvertCSharpClassToTypeScript
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private string _inputPath;
        private string _outputPath;

        public void Run()
        {
            Console.WriteLine("Enter Input Path");
            var path = Console.ReadLine();

            while (!Directory.Exists(path))
            {
                Console.WriteLine($"Cannot find folder: {path}");
                Console.WriteLine("Enter Input Path");
                path = Console.ReadLine();
            }

            _inputPath = path;

            Console.WriteLine("Enter Output Path");
            path = Console.ReadLine();

            while (!Directory.Exists(path))
            {
                Console.WriteLine($"Cannot find folder: {path}");
                Console.WriteLine("Enter Output Path");
                path = Console.ReadLine();
            }

            _outputPath = path;

            var files = Directory.GetFiles(_inputPath, "*.cs");
            var cache = new ClassDefinitionCache();
            var classes = new Dictionary<string, ClassDefinition>();

            Console.WriteLine($"Total {files.Length} files to create");
            Console.WriteLine("Reading class meta data");

            foreach (var file in files)
            {
                var stream = File.OpenRead(file);
                var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                var s = CharStreams.fromstring(content);
                var lexer = new CSharpLexer(s);
                var tokens = new CommonTokenStream(lexer);
                var parser = new CSharpParser(tokens) { BuildParseTree = true };
                var context = parser.compilation_unit();

                var classVisitor = new ClassDefinitionVisitor(cache);
                var classDefinitionCollection = classVisitor.Visit(context);
                classDefinitionCollection.ForEach(c=>classes.Add(c.Name, c));
            }

            Console.WriteLine("Done parsing");
            Console.WriteLine("Creating files");

            foreach (var c in classes.Values)
            {
                CreateTsFile(classes, c);
            }
            
            Console.WriteLine("All files converted");
            Console.WriteLine("Press any to escape");
            Console.Read();
        }

        private void CreateTsFile(IReadOnlyDictionary<string, ClassDefinition> classes, ClassDefinition c)
        {
            var builder = new StringBuilder();
            var imports = c.References.Where(classes.ContainsKey);

            foreach (var import in imports)
            {
                builder.AppendLine($@"import {{ {import} }} from './{import}'");
            }

            builder.AppendLine().AppendLine($"export class {c.Name} {{");

            foreach (var property in c.Properties)
            {
                builder.AppendLine($"    {property.Name}: {property.Type.TypeScriptName};");
            }

            builder.AppendLine("}");

            var result = builder.ToString();
            File.WriteAllText($@"{_outputPath}\{c.Name}.ts", result, Encoding.UTF8);
            Console.WriteLine($"File {c.Name}.ts created");
        }
    }
}