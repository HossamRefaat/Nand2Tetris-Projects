using System.Collections.Generic;

namespace Assembler
{
    /// <summary>
    /// Manages symbol table for the assembler, handling predefined symbols and user-defined symbols
    /// </summary>
    public class SymbolTable : ISymbolTable
    {
        private readonly Dictionary<string, int> _symbols;
        private int _nextVariableAddress;

        public SymbolTable()
        {
            _nextVariableAddress = 16; // Variables start at address 16
            _symbols = InitializePredefinedSymbols();
        }

        public void AddSymbol(string symbol, int address)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol cannot be null or empty", nameof(symbol));
            
            _symbols[symbol] = address;
        }

        public bool ContainsSymbol(string symbol)
        {
            return !string.IsNullOrWhiteSpace(symbol) && _symbols.ContainsKey(symbol);
        }

        public int GetAddress(string symbol)
        {
            if (!ContainsSymbol(symbol))
                throw new ArgumentException($"Symbol '{symbol}' not found in symbol table", nameof(symbol));
            
            return _symbols[symbol];
        }

        public int GetNextVariableAddress()
        {
            return _nextVariableAddress++;
        }

        private static Dictionary<string, int> InitializePredefinedSymbols()
        {
            return new Dictionary<string, int>
            {
                // Registers
                {"R0", 0}, {"R1", 1}, {"R2", 2}, {"R3", 3}, {"R4", 4},
                {"R5", 5}, {"R6", 6}, {"R7", 7}, {"R8", 8}, {"R9", 9},
                {"R10", 10}, {"R11", 11}, {"R12", 12}, {"R13", 13}, {"R14", 14}, {"R15", 15},
                
                // Special addresses
                {"SCREEN", 16384},
                {"KBD", 24576},
                
                // Virtual registers
                {"SP", 0},    // Stack pointer
                {"LCL", 1},   // Local
                {"ARG", 2},   // Argument
                {"THIS", 3},  // This
                {"THAT", 4}   // That
            };
        }
    }
}
