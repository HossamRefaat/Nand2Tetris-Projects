using System;

namespace Assembler
{
    /// <summary>
    /// Main assembler class that coordinates the assembly process
    /// </summary>
    public class HackAssembler
    {
        private readonly IAssemblyParser _parser;

        public HackAssembler() : this(CreateDefaultParser())
        {
        }

        public HackAssembler(IAssemblyParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Assembles a Hack assembly file into machine code
        /// </summary>
        /// <param name="inputFilePath">Path to the input assembly file</param>
        /// <param name="outputFilePath">Path to the output machine code file</param>
        public void AssembleFile(string inputFilePath, string outputFilePath)
        {
            try
            {
                _parser.ParseFile(inputFilePath, outputFilePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Assembly failed: {ex.Message}", ex);
            }
        }

        private static IAssemblyParser CreateDefaultParser()
        {
            var symbolTable = new SymbolTable();
            var instructionTranslator = new InstructionTranslator();
            return new AssemblyParser(symbolTable, instructionTranslator);
        }
    }
}
