namespace Assembler
{
    /// <summary>
    /// Interface for translating assembly instructions to machine code
    /// </summary>
    public interface IInstructionTranslator
    {
        /// <summary>
        /// Translates an A-instruction to binary
        /// </summary>
        string TranslateAInstruction(string instruction, ISymbolTable symbolTable);
        
        /// <summary>
        /// Translates a C-instruction to binary
        /// </summary>
        string TranslateCInstruction(string instruction);
        
        /// <summary>
        /// Determines if an instruction is an A-instruction
        /// </summary>
        bool IsAInstruction(string instruction);
        
        /// <summary>
        /// Determines if an instruction is a label
        /// </summary>
        bool IsLabel(string instruction);
    }
}
