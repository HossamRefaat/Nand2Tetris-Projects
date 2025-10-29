using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Translator.Implementation
{
    public class CodeWriter
    {
        private StreamWriter writer;
        private int labelCounter;
        private string fileName;
        private string currentFunction;

        public CodeWriter(string outputFilePath, string fileName)
        {
            writer = new StreamWriter(outputFilePath);
            this.fileName = fileName;
            labelCounter = 0;
            currentFunction = "";
        }

        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        public void SetCurrentFunction(string functionName)
        {
            currentFunction = functionName;
        }

        public void Close()
        {
            writer.Close();
        }

        private void WriteLine(string line)
        {
            writer.WriteLine(line);
        }

        // Called for arithmetic/logic operations
        public void WriteArithmetic(string command)
        {
            switch (command)
            {
                case "add": WriteAdd(); break;
                case "sub": WriteSub(); break;
                case "neg": WriteNeg(); break;
                case "eq": WriteEq(); break;
                case "gt": WriteGt(); break;
                case "lt": WriteLt(); break;
                case "and": WriteAnd(); break;
                case "or": WriteOr(); break;
                case "not": WriteNot(); break;
            }
        }

        // Called for push/pop
        public void WritePushPop(CommandType type, string segment, int index)
        {
            if (type == CommandType.C_PUSH)
                WritePush(segment, index);
            else if (type == CommandType.C_POP)
                WritePop(segment, index);
        }

        //constant, (local, argument, this, that), static, temp, pointer
        private void WritePush(string segment, int index)
        {
            switch (segment)
            {
                case "constant": WritePushConstant(index); break;
                case "static": WritePushStatic(index); break;
                case "temp": WirtePushTemp(index); break;
                case "pointer": WritePushPointer(index); break;
                default: GlobalWritePush(segment, index); break;
            }
        }

        private void WritePop(string segment, int index)
        {
            switch (segment)
            {
                case "static": WritePopStatic(index); break;
                case "temp": WirtePopTemp(index); break;
                case "pointer": WritePopPointer(index); break;
                default: GlobalWritePop(segment, index); break;
            }
        }

        private void GlobalWritePop(string segment, int index)
        {
            string variable = segment switch
            {
                "local" => "LCL",
                "argument" => "ARG",
                "this" => "THIS",
                "that" => "THAT",
                _ => throw new InvalidOperationException($"Invalid segment: {segment}")
            };

            WriteLine($"// pop {segment} {index}");
            WriteLine($"@{variable}");
            WriteLine("D=M");
            WriteLine($"@{index}");
            WriteLine("D=D+A");
            WriteLine("@addr");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@addr");
            WriteLine("A=M");
            WriteLine("M=D");
        }

        private void WritePopPointer(int index)
        {
            string thisOrThat = index == 0 ? "THIS" : "THAT";
            WriteLine($"// pop pointer {index}");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine($"@{thisOrThat}");
            WriteLine("M=D");
        }

        private void WirtePopTemp(int index)
        {
            WriteLine($"// pop temp {index}");
            WriteLine($"@{index}");
            WriteLine("D=A");
            WriteLine("@5");
            WriteLine("D=D+A");
            WriteLine("@addr");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@addr");
            WriteLine("A=M");
            WriteLine("M=D");
        }

        private void WritePopStatic(int index)
        {
            WriteLine($"//pop static {index}");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine($"@{fileName}.{index}");
            WriteLine("M=D");
        }

        private void GlobalWritePush(string segment, int index)
        {
            string variable = segment switch
            {
                "local" => "LCL",
                "argument" => "ARG",
                "this" => "THIS",
                "that" => "THAT",
                _ => throw new InvalidOperationException($"Invalid segment: {segment}")
            };

            WriteLine($"// push {segment} {index}");
            WriteLine($"@{variable}");
            WriteLine("D=M");
            WriteLine($"@{index}");
            WriteLine("D=D+A");
            WriteLine("@addr");
            WriteLine("M=D");
            WriteLine("@addr");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WritePushPointer(int index)
        {
            string thisOrThat = index == 0 ? "THIS" : "THAT";
            WriteLine($"// push pointer {index}");
            WriteLine($"@{thisOrThat}");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WirtePushTemp(int index)
        {
            WriteLine($"//push temp {index}");
            WriteLine($"@{index}");
            WriteLine("D=A");
            WriteLine("@5");
            WriteLine("D=D+A");
            WriteLine("@addr");
            WriteLine("M=D");
            WriteLine("@addr");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WritePushStatic(int index)
        {
            WriteLine($"//push static {index}");
            WriteLine($"@{fileName}.{index}");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WritePushConstant(int index)
        {
            WriteLine($"//push constant {index}");
            WriteLine($"@{index}");
            WriteLine("D=A");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        public void WriteAdd()
        {
            WriteLine("//add");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=D+M");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteSub()
        {
            WriteLine("//sub");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=M-D");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteAnd()
        {
            WriteLine("//and");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=D&M");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteOr()
        {
            WriteLine("//or");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=D|M");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteNeg()
        {
            WriteLine("//neg");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=-M");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteNot()
        {
            WriteLine("//not");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("M=!M");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteEq()
        {
            string labelTrue = $"EQ_TRUE{labelCounter}";
            string labelEnd = $"EQ_END{labelCounter}";
            labelCounter++;
            WriteLine("//eq");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M-D");
            WriteLine($"@{labelTrue}");
            WriteLine("D;JEQ");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=0"); // false
            WriteLine($"@{labelEnd}");
            WriteLine("0;JMP");
            WriteLine($"({labelTrue})");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=-1"); // true
            WriteLine($"({labelEnd})");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteGt()
        {
            string labelTrue = $"GT_TRUE{labelCounter}";
            string labelEnd = $"GT_END{labelCounter}";
            labelCounter++;
            WriteLine("//gt");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M-D");
            WriteLine($"@{labelTrue}");
            WriteLine("D;JGT");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=0"); // false
            WriteLine($"@{labelEnd}");
            WriteLine("0;JMP");
            WriteLine($"({labelTrue})");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=-1"); // true
            WriteLine($"({labelEnd})");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        private void WriteLt()
        {
            string labelTrue = $"LT_TRUE{labelCounter}";
            string labelEnd = $"LT_END{labelCounter}";
            labelCounter++;
            WriteLine("//lt");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M-D");
            WriteLine($"@{labelTrue}");
            WriteLine("D;JLT");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=0"); // false
            WriteLine($"@{labelEnd}");
            WriteLine("0;JMP");
            WriteLine($"({labelTrue})");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=-1"); // true
            WriteLine($"({labelEnd})");
            WriteLine("@SP");
            WriteLine("M=M+1");
        }

        internal void WriteLabel(string label)
        {
            WriteLine($"// label {label}");
            WriteLine($"({currentFunction}${label})");
        }

        internal void WriteGoto(string label)
        {
            WriteLine($"// goto {label}");
            WriteLine($"@{currentFunction}${label}");
            WriteLine("0;JMP");
        }

        internal void WriteIf(string label)
        {
            WriteLine($"// if-goto {label}");
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine($"@{currentFunction}${label}");
            WriteLine("D;JNE");
        }

        internal void WriteFunction(string functionName, int numLocals)
        {
            WriteLine($"// function {functionName} {numLocals}");
            WriteLine($"({functionName})");

            currentFunction = functionName;

            if (numLocals > 0)
            {
                string loopLabel = $"{functionName}$INIT_LOCALS";
                string endLabel = $"{functionName}$END_INIT";

                // D = numLocals
                WriteLine($"@{numLocals}");
                WriteLine("D=A");
                WriteLine($"({loopLabel})");
                WriteLine("@SP");
                WriteLine("A=M");
                WriteLine("M=0");
                WriteLine("@SP");
                WriteLine("M=M+1");
                WriteLine("D=D-1");
                WriteLine($"@{loopLabel}");
                WriteLine("D;JGT");
                //WriteLine($"({endLabel})");
            }
        }

        internal void WriteCall(string functionName, int numArgs)
        {
            string returnLabel = $"RETURN_{functionName}_{labelCounter++}";

            WriteLine($"// call {functionName} {numArgs}");

            // 1. Push return-address
            WriteLine($"@{returnLabel}");
            WriteLine("D=A");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");

            // 2. Push LCL
            WriteLine("@LCL");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");

            // 3. Push ARG
            WriteLine("@ARG");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");

            // 4. Push THIS
            WriteLine("@THIS");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");

            // 5. Push THAT
            WriteLine("@THAT");
            WriteLine("D=M");
            WriteLine("@SP");
            WriteLine("A=M");
            WriteLine("M=D");
            WriteLine("@SP");
            WriteLine("M=M+1");

            // 6. ARG = SP - numArgs - 5
            WriteLine("@SP");
            WriteLine("D=M");
            WriteLine($"@{numArgs + 5}");
            WriteLine("D=D-A");
            WriteLine("@ARG");
            WriteLine("M=D");

            // 7. LCL = SP
            WriteLine("@SP");
            WriteLine("D=M");
            WriteLine("@LCL");
            WriteLine("M=D");

            // 8. goto functionName
            WriteLine($"@{functionName}");
            WriteLine("0;JMP");

            // 9. (return-label)
            WriteLine($"({returnLabel})");
        }

        internal void WriteReturn()
        {
            WriteLine("// return");

            // endFrame = LCL
            WriteLine("@LCL");
            WriteLine("D=M");
            WriteLine("@endFrame");
            WriteLine("M=D");

            // retAddr = *(endFrame - 5)
            WriteLine("@endFrame");
            WriteLine("D=M");
            WriteLine("@5");
            WriteLine("A=D-A");
            WriteLine("D=M");
            WriteLine("@retAddr");
            WriteLine("M=D");

            // *ARG = pop()
            WriteLine("@SP");
            WriteLine("M=M-1");
            WriteLine("A=M");
            WriteLine("D=M");
            WriteLine("@ARG");
            WriteLine("A=M");
            WriteLine("M=D");

            //SP = ARG + 1
            WriteLine("@ARG");
            WriteLine("D=M+1");
            WriteLine("@SP");
            WriteLine("M=D");

            // THAT = *(endFrame - 1)
            WriteLine("@endFrame");
            WriteLine("D=M");
            WriteLine("@1");
            WriteLine("A=D-A");
            WriteLine("D=M");
            WriteLine("@THAT");
            WriteLine("M=D");

            // THIS = *(endFrame - 2)
            WriteLine("@endFrame");
            WriteLine("D=M");
            WriteLine("@2");
            WriteLine("A=D-A");
            WriteLine("D=M");
            WriteLine("@THIS");
            WriteLine("M=D");

            // ARG = *(endFrame - 3)
            WriteLine("@endFrame");
            WriteLine("D=M");
            WriteLine("@3");
            WriteLine("A=D-A");
            WriteLine("D=M");
            WriteLine("@ARG");
            WriteLine("M=D");

            // LCL = *(endFrame - 4)
            WriteLine("@endFrame");
            WriteLine("D=M");
            WriteLine("@4");
            WriteLine("A=D-A");
            WriteLine("D=M");
            WriteLine("@LCL");
            WriteLine("M=D");

            //goto retAddr
            WriteLine("@retAddr");
            WriteLine("A=M");
            WriteLine("0;JMP");
        }

        internal void WriteInit()
        {
            WriteLine("// bootstrap code");
            WriteLine("@256");
            WriteLine("D=A");
            WriteLine("@SP");
            WriteLine("M=D");
            WriteCall("Sys.init", 0);   
        }

    }
}
