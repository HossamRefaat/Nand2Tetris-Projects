using System.IO;

namespace Assembler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: Assembler <input_file.asm>");
                    Console.WriteLine("Example: Assembler program.asm");
                    Environment.Exit(1);
                }

                string inputFilePath = args[0];
                
                if (!File.Exists(inputFilePath))
                {
                    Console.WriteLine($"Error: Input file '{inputFilePath}' not found.");
                    Environment.Exit(1);
                }

                // Generate output file path in same directory with _out suffix
                string directory = Path.GetDirectoryName(inputFilePath) ?? "";
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
                string extension = Path.GetExtension(inputFilePath);
                string outputFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_out{extension}");

                var assembler = new HackAssembler();
                assembler.AssembleFile(inputFilePath, outputFilePath);

                Console.WriteLine($"Assembly complete. Output written to '{outputFilePath}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
