using JackCompiler.Abstractions;
using JackCompiler.Implementions;

namespace JackCompiler
{
    public class JackCompiler
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: JackCompiler <input path>");
                return;
            }

            string inputPath = args[0];

            if (File.Exists(inputPath))
            {
                CompileFile(inputPath);
            }
            else if (Directory.Exists(inputPath))
            {
                foreach (var file in Directory.GetFiles(inputPath, "*.jack"))
                {
                    CompileFile(file);
                }
            }
            else
            {
                Console.WriteLine("Invalid path.");
            }
        }

        private static void CompileFile(string filePath)
        {
            using var vmWriter = new VMWriter(GetOutputPath(filePath));
            IJackTokenizer tokenizer = new JackTokenizer(filePath);
            ISymbolTable symbolTable = new SymbolTable();

            // Advance to the first token before starting compilation
            tokenizer.Advance();

            var engine = new CompilationEngine(tokenizer, symbolTable, vmWriter);

            engine.CompileClass();
        }

        private static string GetOutputPath(string jackFile)
        {
            string directory = Path.GetDirectoryName(jackFile) ?? "";
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jackFile);
            string outputFileName = $"{fileNameWithoutExtension}.vm";
            return Path.Combine(directory, outputFileName);
        }
    }
}