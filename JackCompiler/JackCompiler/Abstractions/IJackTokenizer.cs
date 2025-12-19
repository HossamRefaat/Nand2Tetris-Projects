using JackCompiler.Enums;

namespace JackCompiler.Abstractions
{
    public interface IJackTokenizer
    {
        public bool HasMoreTokens();
        public void Advance();
        public void StepBack();

        TokenType CurrentTokenType { get; }

        Keywords Keyword();
        char Symbol();
        string Identifier();
        int IntVal();
        string StringVal();

    }
}
