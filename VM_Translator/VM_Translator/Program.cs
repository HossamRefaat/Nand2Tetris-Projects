using VM_Translator.Implementation;

namespace VM_Translator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: VMTranslator <input_file.vm> OR <directory>");
                    Environment.Exit(1);
                }

                string inputPath = args[0];

                if (Directory.Exists(inputPath))
                {
                    TranslateDirectory(inputPath);
                }
                else if (File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".vm", StringComparison.OrdinalIgnoreCase))
                {
                    TranslateSingleFile(inputPath);
                }
                else
                {
                    Console.WriteLine("Error: Input path is neither a valid directory nor a .vm file.");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void TranslateDirectory(string inputPath)
        {
            string[] vmFiles = Directory.GetFiles(inputPath, "*.vm");
            string directoryName = new DirectoryInfo(inputPath).Name;
            string outputFilePath = Path.Combine(inputPath, $"{directoryName}.asm");

            var codeWriter = new CodeWriter(outputFilePath, directoryName);

            // Bootstrap code (SP = 256, call Sys.init)
            codeWriter.WriteInit();

            foreach (var vmFile in vmFiles)
                TranslateFile(vmFile, codeWriter);

            codeWriter.Close();
            Console.WriteLine($"Assembly complete. Output written to '{outputFilePath}'");
        }

        private static void TranslateSingleFile(string inputFilePath)
        {
            string directory = Path.GetDirectoryName(inputFilePath) ?? "";
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
            string outputFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}.asm");

            var codeWriter = new CodeWriter(outputFilePath, fileNameWithoutExtension);
            TranslateFile(inputFilePath, codeWriter);
            codeWriter.Close();

            Console.WriteLine($"Assembly complete. Output written to '{outputFilePath}'");
        }

        private static void TranslateFile(string filePath, CodeWriter codeWriter)
        {
            codeWriter.SetFileName(Path.GetFileNameWithoutExtension(filePath));
            var parser = new Parser(filePath);

            while (parser.HasMoreCommands())
            {
                parser.Advance();
                var type = parser.CommandTypef();

                switch (type)
                {
                    case CommandType.C_ARITHMETIC:
                        codeWriter.WriteArithmetic(parser.Arg1());
                        break;
                    case CommandType.C_PUSH:
                    case CommandType.C_POP:
                        codeWriter.WritePushPop(type, parser.Arg1(), parser.Arg2());
                        break;
                    case CommandType.C_LABEL:
                        codeWriter.WriteLabel(parser.Arg1());
                        break;
                    case CommandType.C_GOTO:
                        codeWriter.WriteGoto(parser.Arg1());
                        break;
                    case CommandType.C_IF:
                        codeWriter.WriteIf(parser.Arg1());
                        break;
                    case CommandType.C_FUNCTION:
                        codeWriter.WriteFunction(parser.Arg1(), parser.Arg2());
                        break;
                    case CommandType.C_CALL:
                        codeWriter.WriteCall(parser.Arg1(), parser.Arg2());
                        break;
                    case CommandType.C_RETURN:
                        codeWriter.WriteReturn();
                        break;
                }
            }
        }
    }
}
