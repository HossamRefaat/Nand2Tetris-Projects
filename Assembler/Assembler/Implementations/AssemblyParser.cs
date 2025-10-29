using System;
using System.IO;

namespace Assembler
{
    /// <summary>
    /// Parses Hack assembly files and generates machine code
    /// </summary>
    public class AssemblyParser : IAssemblyParser
    {
        private readonly ISymbolTable _symbolTable;
        private readonly IInstructionTranslator _instructionTranslator;

        public AssemblyParser(ISymbolTable symbolTable, IInstructionTranslator instructionTranslator)
        {
            _symbolTable = symbolTable ?? throw new ArgumentNullException(nameof(symbolTable));
            _instructionTranslator = instructionTranslator ?? throw new ArgumentNullException(nameof(instructionTranslator));
        }

        public void ParseFile(string inputFilePath, string outputFilePath)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentException("Input file path cannot be null or empty", nameof(inputFilePath));
            
            if (string.IsNullOrWhiteSpace(outputFilePath))
                throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFilePath));

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"Input file not found: {inputFilePath}");

            // First pass: collect labels and their addresses
            FirstPass(inputFilePath);

            // Second pass: generate machine code
            SecondPass(inputFilePath, outputFilePath);
        }

        private void FirstPass(string inputFilePath)
        {
            int instructionAddress = 0;

            foreach (string rawLine in File.ReadLines(inputFilePath))
            {
                string cleanLine = CleanLine(rawLine);
                
                if (string.IsNullOrEmpty(cleanLine))
                    continue;

                if (_instructionTranslator.IsLabel(cleanLine))
                {
                    // Extract label name and add to symbol table
                    string labelName = cleanLine.Substring(1, cleanLine.Length - 2);
                    _symbolTable.AddSymbol(labelName, instructionAddress);
                }
                else
                {
                    // Only increment address for actual instructions
                    instructionAddress++;
                }
            }
        }

        private void SecondPass(string inputFilePath, string outputFilePath)
        {
            using var writer = new StreamWriter(outputFilePath);
            
            foreach (string rawLine in File.ReadLines(inputFilePath))
            {
                string cleanLine = CleanLine(rawLine);
                
                if (string.IsNullOrEmpty(cleanLine) || _instructionTranslator.IsLabel(cleanLine))
                    continue;

                string machineCode = TranslateInstruction(cleanLine);
                writer.WriteLine(machineCode);
            }
        }

        private string TranslateInstruction(string instruction)
        {
            if (_instructionTranslator.IsAInstruction(instruction))
            {
                return _instructionTranslator.TranslateAInstruction(instruction, _symbolTable);
            }
            else
            {
                return _instructionTranslator.TranslateCInstruction(instruction);
            }
        }

        private static string CleanLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return string.Empty;

            // Remove comments and trim whitespace
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                line = line.Substring(0, commentIndex);
            }

            return line.Trim();
        }
    }
}
