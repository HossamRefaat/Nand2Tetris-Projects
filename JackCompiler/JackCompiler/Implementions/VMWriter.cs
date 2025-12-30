using JackCompiler.Enums;

namespace JackCompiler.Implementions
{
    internal class VMWriter : IVMWriter, IDisposable
    {
        private readonly StreamWriter _writer;

        public VMWriter(string outputPath)
        {
            _writer = new StreamWriter(outputPath);
        }

        public void WritePush(VMSegment segment, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (segment == VMSegment.POINTER && index > 1)
                throw new ArgumentException("Pointer segment index must be 0 or 1.");

            if (segment == VMSegment.TEMP && index > 7)
                throw new ArgumentException("Temp segment index must be between 0 and 7.");

            string segmentText = segment switch
            {
                VMSegment.CONST => "constant",
                VMSegment.ARG => "argument",
                VMSegment.LOCAL => "local",
                VMSegment.STATIC => "static",
                VMSegment.THIS => "this",
                VMSegment.THAT => "that",
                VMSegment.POINTER => "pointer",
                VMSegment.TEMP => "temp",
                _ => throw new ArgumentOutOfRangeException(nameof(segment))
            };

            _writer.WriteLine($"push {segmentText} {index}");
        }

        public void WritePop(VMSegment segment, int index)
        {
            if (segment == VMSegment.CONST)
                throw new ArgumentException("Cannot pop to constant segment.");

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (segment == VMSegment.POINTER && index > 1)
                throw new ArgumentException("Pointer segment index must be 0 or 1.");

            if (segment == VMSegment.TEMP && index > 7)
                throw new ArgumentException("Temp segment index must be between 0 and 7.");

            string segmentText = segment switch
            {
                VMSegment.ARG => "argument",
                VMSegment.LOCAL => "local",
                VMSegment.STATIC => "static",
                VMSegment.THIS => "this",
                VMSegment.THAT => "that",
                VMSegment.POINTER => "pointer",
                VMSegment.TEMP => "temp",
                _ => throw new ArgumentOutOfRangeException(nameof(segment))
            };

            _writer.WriteLine($"pop {segmentText} {index}");
        }

        public void WriteArithmetic(VMArithmetic command)
        {
            string vmCommand = command switch
            {
                VMArithmetic.ADD => "add",
                VMArithmetic.SUB => "sub",
                VMArithmetic.NEG => "neg",
                VMArithmetic.EQ => "eq",
                VMArithmetic.GT => "gt",
                VMArithmetic.LT => "lt",
                VMArithmetic.AND => "and",
                VMArithmetic.OR => "or",
                VMArithmetic.NOT => "not",
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };

            _writer.WriteLine(vmCommand);
        }

        public void WriteLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));

            // VM labels cannot contain spaces
            if (label.Contains(' '))
                throw new ArgumentException("Label cannot contain spaces.", nameof(label));

            _writer.WriteLine($"label {label}");
        }

        public void WriteGoto(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));

            // VM labels cannot contain spaces
            if (label.Contains(' '))
                throw new ArgumentException("Label cannot contain spaces.", nameof(label));

            _writer.WriteLine($"goto {label}");
        }

        public void WriteIf(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));

            // VM labels cannot contain spaces
            if (label.Contains(' '))
                throw new ArgumentException("Label cannot contain spaces.", nameof(label));

            _writer.WriteLine($"if-goto {label}");
        }

        public void WriteCall(string name, int argCount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(name));

            if (argCount < 0)
                throw new ArgumentOutOfRangeException(nameof(argCount));

            _writer.WriteLine($"call {name} {argCount}");
        }

        public void WriteFunction(string name, int localCount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(name));

            if (localCount < 0)
                throw new ArgumentOutOfRangeException(nameof(localCount));

            _writer.WriteLine($"function {name} {localCount}");
        }

        public void WriteReturn()
        {
            _writer.WriteLine("return");
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
