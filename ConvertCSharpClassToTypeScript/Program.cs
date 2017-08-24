using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var step1 = new Step("Enter Input Path");
            var step2 = new Step("Enter Output Path");

            step1.Result += (sender, inputPath) =>
            {
                if (!Directory.Exists(inputPath))
                {
                    Console.WriteLine($"Cannot find folder: {inputPath}");
                    step1.ShowAndWatch();
                }
                else
                {
                    _inputPath = inputPath;
                    step2.ShowAndWatch();
                }
            };

            step2.Result += (sender, outputPath) =>
            {
                if (!Directory.Exists(outputPath))
                {
                    Console.WriteLine($"Cannot find folder: {outputPath}");
                    step2.ShowAndWatch();
                }
                else
                {
                    _outputPath = outputPath;
                    ConvertFiles();
                }
            };

            step1.ShowAndWatch();
        }

        private void ConvertFiles()
        {
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
                var parser = new CSharpParser(tokens) {BuildParseTree = true};
                var context = parser.compilation_unit();

                var classVisitor = new ClassDefinitionVisitor(cache);
                var classDefinitionCollection = classVisitor.Visit(context);
                classDefinitionCollection.ForEach(c => classes.Add(c.Name, c));
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