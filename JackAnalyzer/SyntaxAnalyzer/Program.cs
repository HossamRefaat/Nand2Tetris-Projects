using SyntaxAnalyzer.Implementions;

public class JackAnalyzer
{
    /*
     some notes
        1. the output .xml file has to start and end with <tokens></tokens>
        2. string costants are outputted without the double quotes  
        3. <, >, ", and & have to be replaced with &lt;, &gt;, &quot;, and &amp; respectively
     */
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: JackAnalyzer <input path>");
            return;
        }

        string inputPath = args[0];

        if (File.Exists(inputPath))
        {
            AnalyzeFile(inputPath);
        }
        else if (Directory.Exists(inputPath))
        {
            foreach (var file in Directory.GetFiles(inputPath, "*.jack"))
            {
                AnalyzeFile(file);
            }
        }
        else
        {
            Console.WriteLine("Invalid path.");
        }
    }

    private static void AnalyzeFile(string filePath)
    {
        var tokenizer = new JackTokenizer(filePath);
        using var engine = new CompilationEngine(tokenizer, GetOutputPath(filePath));

        engine.CompileClass();
    }

    private static string GetOutputPath(string jackFile)
    {
        string directory = Path.GetDirectoryName(jackFile) ?? "";
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jackFile);
        string outputFileName = $"{fileNameWithoutExtension}Out.xml";
        return Path.Combine(directory, outputFileName);
    }
}
