namespace Assembler
{
    /// <summary>
    /// Interface for managing symbol table operations in the assembler
    /// </summary>
    public interface ISymbolTable
    {
        /// <summary>
        /// Adds a symbol to the table with the specified address
        /// </summary>
        void AddSymbol(string symbol, int address);
        
        /// <summary>
        /// Checks if a symbol exists in the table
        /// </summary>
        bool ContainsSymbol(string symbol);
        
        /// <summary>
        /// Gets the address associated with a symbol
        /// </summary>
        int GetAddress(string symbol);
        
        /// <summary>
        /// Gets the next available variable address
        /// </summary>
        int GetNextVariableAddress();
    }
}
