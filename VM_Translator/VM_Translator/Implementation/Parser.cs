using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Translator.Implementation
{

    public class Parser
    {
        private readonly List<string> commands;
        private int currentCommandIndex = -1;
        private string currentCommand = string.Empty;

        public Parser(string filePath)
        {
            // Read file and preprocess (remove comments, trim whitespace)
            commands = File.ReadAllLines(filePath)
                .Select(line => line.Split("//")[0].Trim())  // remove comments
                .Where(line => !string.IsNullOrWhiteSpace(line)) // skip empty lines
                .ToList();
        }

        public bool HasMoreCommands() => currentCommandIndex + 1 < commands.Count;

        public void Advance()
        {
            if (HasMoreCommands())
            {
                currentCommandIndex++;
                currentCommand = commands[currentCommandIndex];
            }
        }

        public CommandType CommandTypef()
        {
            if (currentCommand.StartsWith("push")) return CommandType.C_PUSH;
            if (currentCommand.StartsWith("pop")) return CommandType.C_POP;
            if (currentCommand.StartsWith("label")) return CommandType.C_LABEL;
            if (currentCommand.StartsWith("goto")) return CommandType.C_GOTO;
            if (currentCommand.StartsWith("if-goto")) return CommandType.C_IF;
            if (currentCommand.StartsWith("function")) return CommandType.C_FUNCTION;
            if (currentCommand.StartsWith("call")) return CommandType.C_CALL;
            if (currentCommand.StartsWith("return")) return CommandType.C_RETURN;

            // Default: arithmetic/logical
            return CommandType.C_ARITHMETIC;
        }


        //command [arg1] [arg2]
        public string Arg1()
        {
            var parts = currentCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var type = CommandTypef();

            if (type == CommandType.C_RETURN)
                throw new InvalidOperationException("C_RETURN command has no arguments.");

            if (type == CommandType.C_ARITHMETIC)
                return parts[0]; // e.g., add, sub, neg, eq, gt, lt, etc.

            return parts.Length > 1 ? parts[1] : string.Empty;
        }

        public int Arg2()
        {
            var parts = currentCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var type = CommandTypef();

            if (type == CommandType.C_PUSH ||
                type == CommandType.C_POP ||
                type == CommandType.C_FUNCTION ||
                type == CommandType.C_CALL)
            {
                return int.Parse(parts[2]);
            }

            throw new InvalidOperationException($"Command {type} does not have an Arg2.");
        }

    }

}
