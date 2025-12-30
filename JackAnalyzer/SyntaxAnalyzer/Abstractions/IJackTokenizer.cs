using SyntaxAnalyzer.Enums;

namespace SyntaxAnalyzer.Abstractions
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
