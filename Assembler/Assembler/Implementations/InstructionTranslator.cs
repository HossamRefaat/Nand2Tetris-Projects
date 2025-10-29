using System;
using System.Collections.Generic;

namespace Assembler
{
    /// <summary>
    /// Translates Hack assembly instructions to machine code
    /// </summary>
    public class InstructionTranslator : IInstructionTranslator
    {
        private readonly Dictionary<string, string> _computationCodes;
        private readonly Dictionary<string, string> _destinationCodes;
        private readonly Dictionary<string, string> _jumpCodes;

        public InstructionTranslator()
        {
            _computationCodes = InitializeComputationCodes();
            _destinationCodes = InitializeDestinationCodes();
            _jumpCodes = InitializeJumpCodes();
        }

        public string TranslateAInstruction(string instruction, ISymbolTable symbolTable)
        {
            if (!IsAInstruction(instruction))
                throw new ArgumentException("Invalid A-instruction format", nameof(instruction));

            string value = instruction.Substring(1); // Remove '@' prefix
            int address;

            if (int.TryParse(value, out address))
            {
                // Numeric constant
                return ConvertToBinary(address);
            }
            else
            {
                // Symbol reference
                if (!symbolTable.ContainsSymbol(value))
                {
                    // New variable - assign next available address
                    address = symbolTable.GetNextVariableAddress();
                    symbolTable.AddSymbol(value, address);
                }
                else
                {
                    address = symbolTable.GetAddress(value);
                }
                
                return ConvertToBinary(address);
            }
        }

        public string TranslateCInstruction(string instruction)
        {
            if (IsAInstruction(instruction) || IsLabel(instruction))
                throw new ArgumentException("Invalid C-instruction format", nameof(instruction));

            var parts = ParseCInstruction(instruction);
            
            if (!_computationCodes.ContainsKey(parts.Computation))
                throw new ArgumentException($"Invalid computation: {parts.Computation}");
            
            if (!_destinationCodes.ContainsKey(parts.Destination))
                throw new ArgumentException($"Invalid destination: {parts.Destination}");
            
            if (!_jumpCodes.ContainsKey(parts.Jump))
                throw new ArgumentException($"Invalid jump: {parts.Jump}");

            return "111" + _computationCodes[parts.Computation] + 
                   _destinationCodes[parts.Destination] + _jumpCodes[parts.Jump];
        }

        public bool IsAInstruction(string instruction)
        {
            return !string.IsNullOrWhiteSpace(instruction) && instruction.StartsWith("@");
        }

        public bool IsLabel(string instruction)
        {
            return !string.IsNullOrWhiteSpace(instruction) && 
                   instruction.StartsWith("(") && instruction.EndsWith(")");
        }

        private static string ConvertToBinary(int value)
        {
            if (value < 0 || value > 32767) // 15-bit address space
                throw new ArgumentOutOfRangeException(nameof(value), "Address must be between 0 and 32767");
            
            return Convert.ToString(value, 2).PadLeft(16, '0');
        }

        private (string Destination, string Computation, string Jump) ParseCInstruction(string instruction)
        {
            string destination = "";
            string computation = instruction;
            string jump = "";

            // Parse destination (before '=')
            if (instruction.Contains("="))
            {
                var equalsParts = instruction.Split('=');
                destination = equalsParts[0].Trim();
                computation = equalsParts[1].Trim();
            }

            // Parse jump (after ';')
            if (computation.Contains(";"))
            {
                var semicolonParts = computation.Split(';');
                computation = semicolonParts[0].Trim();
                jump = semicolonParts[1].Trim();
            }

            return (destination, computation, jump);
        }

        private static Dictionary<string, string> InitializeComputationCodes()
        {
            return new Dictionary<string, string>
            {
                // a = 0 computations
                {"0", "0101010"}, {"1", "0111111"}, {"-1", "0111010"},
                {"D", "0001100"}, {"A", "0110000"}, {"!D", "0001101"},
                {"!A", "0110001"}, {"-D", "0001111"}, {"-A", "0110011"},
                {"D+1", "0011111"}, {"A+1", "0110111"}, {"D-1", "0001110"},
                {"A-1", "0110010"}, {"D+A", "0000010"}, {"D-A", "0010011"},
                {"A-D", "0000111"}, {"D&A", "0000000"}, {"D|A", "0010101"},

                // a = 1 computations (M instead of A)
                {"M", "1110000"}, {"!M", "1110001"}, {"-M", "1110011"},
                {"M+1", "1110111"}, {"M-1", "1110010"}, {"D+M", "1000010"},
                {"D-M", "1010011"}, {"M-D", "1000111"}, {"D&M", "1000000"},
                {"D|M", "1010101"}
            };
        }

        private static Dictionary<string, string> InitializeDestinationCodes()
        {
            return new Dictionary<string, string>
            {
                {"", "000"},    // No destination
                {"M", "001"},   // Memory
                {"D", "010"},   // D register
                {"MD", "011"},  // Memory and D register
                {"A", "100"},   // A register
                {"AM", "101"},  // A register and Memory
                {"AD", "110"},  // A register and D register
                {"AMD", "111"}  // A register, Memory, and D register
            };
        }

        private static Dictionary<string, string> InitializeJumpCodes()
        {
            return new Dictionary<string, string>
            {
                {"", "000"},    // No jump
                {"JGT", "001"}, // Jump if greater than
                {"JEQ", "010"}, // Jump if equal
                {"JGE", "011"}, // Jump if greater than or equal
                {"JLT", "100"}, // Jump if less than
                {"JNE", "101"}, // Jump if not equal
                {"JLE", "110"}, // Jump if less than or equal
                {"JMP", "111"}  // Unconditional jump
            };
        }
    }
}
