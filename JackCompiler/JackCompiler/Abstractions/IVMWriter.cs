using JackCompiler.Enums;

/// <summary>
/// Defines an abstraction for emitting VM commands.
/// The VMWriter is responsible for translating high-level
/// compilation output into Hack Virtual Machine instructions.
/// </summary>
public interface IVMWriter
{
    /// <summary>
    /// Writes a VM push command.
    /// </summary>
    /// <param name="segment">The memory segment to push from.</param>
    /// <param name="index">The index within the memory segment.</param>
    void WritePush(VMSegment segment, int index);

    /// <summary>
    /// Writes a VM pop command.
    /// </summary>
    /// <param name="segment">The memory segment to pop into.</param>
    /// <param name="index">The index within the memory segment.</param>
    void WritePop(VMSegment segment, int index);

    /// <summary>
    /// Writes a VM arithmetic or logical command.
    /// </summary>
    /// <param name="command">The arithmetic command to write.</param>
    void WriteArithmetic(VMArithmetic command);

    /// <summary>
    /// Writes a VM label command.
    /// </summary>
    /// <param name="label">The label identifier.</param>
    void WriteLabel(string label);

    /// <summary>
    /// Writes a VM goto command.
    /// </summary>
    /// <param name="label">The target label.</param>
    void WriteGoto(string label);

    /// <summary>
    /// Writes a VM if-goto command.
    /// Pops the top value from the stack and jumps if it is not zero.
    /// </summary>
    /// <param name="label">The target label.</param>
    void WriteIf(string label);

    /// <summary>
    /// Writes a VM call command.
    /// </summary>
    /// <param name="name">The name of the function to call.</param>
    /// <param name="argCount">The number of arguments to pass.</param>
    void WriteCall(string name, int argCount);

    /// <summary>
    /// Writes a VM function declaration.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="localCount">The number of local variables.</param>
    void WriteFunction(string name, int localCount);

    /// <summary>
    /// Writes a VM return command.
    /// </summary>
    void WriteReturn();

}
