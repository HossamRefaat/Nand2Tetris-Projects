namespace Assembler
{
    /// <summary>
    /// Interface for parsing assembly files
    /// </summary>
    public interface IAssemblyParser
    {
        /// <summary>
        /// Parses an assembly file and generates machine code
        /// </summary>
        void ParseFile(string inputFilePath, string outputFilePath);
    }
}
