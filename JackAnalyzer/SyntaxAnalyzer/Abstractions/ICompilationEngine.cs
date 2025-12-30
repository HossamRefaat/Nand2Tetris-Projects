namespace SyntaxAnalyzer.Abstractions
{
    public interface ICompilationEngine
    {
        public void CompileClass();
        public void CompileClassVarDec();
        public void CompileSubroutine();
        public void CompileParameterList();
        public void CompileSubroutineBody();
        public void CompileVarDec();
        public void CompileStatements();
        public void CompileDo();
        public void CompileWhile();
        public void CompileReturn();
        public void CompileIf();
        public void CompileLet();
        public void CompileTerm();
        public void CompileExpression();
        public void CompileExpressionList();
    }
}
