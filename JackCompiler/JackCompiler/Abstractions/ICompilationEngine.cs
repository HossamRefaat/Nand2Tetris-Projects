namespace JackCompiler.Abstractions
{
    /// <summary>
    /// Defines an abstraction for compiling Jack source code into VM instructions.
    /// The compilation engine parses Jack grammar constructs and emits corresponding
    /// VM commands using a VM writer.
    /// </summary>
    public interface ICompilationEngine
    {
        /// <summary>
        /// Compiles an entire class.
        /// Entry point of the compilation process.
        /// </summary>
        void CompileClass();

        /// <summary>
        /// Compiles a static or field variable declaration.
        /// </summary>
        void CompileClassVarDec();

        /// <summary>
        /// Compiles a complete subroutine declaration
        /// (constructor, function, or method).
        /// </summary>
        void CompileSubroutine();

        /// <summary>
        /// Compiles a parameter list of a subroutine.
        /// Does not include the enclosing parentheses.
        /// </summary>
        void CompileParameterList();

        /// <summary>
        /// Compiles a subroutine body, including local variable
        /// declarations and statements.
        /// </summary>
        void CompileSubroutineBody(Keywords subroutineKind, string subroutineName);

        /// <summary>
        /// Compiles a local variable declaration.
        /// </summary>
        void CompileVarDec();

        /// <summary>
        /// Compiles a sequence of statements.
        /// Does not consume the enclosing braces.
        /// </summary>
        void CompileStatements();

        /// <summary>
        /// Compiles a let statement.
        /// </summary>
        void CompileLet();

        /// <summary>
        /// Compiles an if statement, including optional else clause.
        /// </summary>
        void CompileIf();

        /// <summary>
        /// Compiles a while statement.
        /// </summary>
        void CompileWhile();

        /// <summary>
        /// Compiles a do statement.
        /// </summary>
        void CompileDo();

        /// <summary>
        /// Compiles a return statement.
        /// </summary>
        void CompileReturn();

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        void CompileExpression();

        /// <summary>
        /// Compiles a single term.
        /// A term may be a constant, variable, array access,
        /// subroutine call, parenthesized expression, or unary expression.
        /// </summary>
        void CompileTerm();

        /// <summary>
        /// Compiles a comma-separated list of expressions.
        /// Returns the number of expressions compiled.
        /// </summary>
        /// <returns>The number of expressions in the list.</returns>
        int CompileExpressionList();
    }

}
