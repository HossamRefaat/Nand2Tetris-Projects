using JackCompiler.Abstractions;
using JackCompiler.Enums;

public static class Helper
{
    public static void ExpectKeyword(IJackTokenizer tokenizer, Keywords expected)
    {
        if (tokenizer.CurrentTokenType != TokenType.KEYWORD ||
            tokenizer.Keyword() != expected)
        {
            throw new InvalidOperationException(
                $"Expected keyword '{expected}', but got {Describe(tokenizer)}");
        }

        tokenizer.Advance();
    }

    public static Keywords ExpectKeyword(IJackTokenizer tokenizer)
    {
        if (tokenizer.CurrentTokenType != TokenType.KEYWORD)
        {
            throw new InvalidOperationException(
                $"Expected keyword, but got {Describe(tokenizer)}");
        }

        var value = tokenizer.Keyword();
        tokenizer.Advance();
        return value;
    }

    public static void ExpectSymbol(IJackTokenizer tokenizer, char expected)
    {
        if (tokenizer.CurrentTokenType != TokenType.SYMBOL ||
            tokenizer.Symbol() != expected)
        {
            throw new InvalidOperationException(
                $"Expected symbol '{expected}', but got {Describe(tokenizer)}");
        }

        tokenizer.Advance();
    }

    public static string ExpectIdentifier(IJackTokenizer tokenizer)
    {
        if (tokenizer.CurrentTokenType != TokenType.IDENTIFIER)
        {
            throw new InvalidOperationException(
                $"Expected identifier, but got {Describe(tokenizer)}");
        }

        string name = tokenizer.Identifier();
        tokenizer.Advance();
        return name;
    }

    public static int ExpectInt(IJackTokenizer tokenizer)
    {
        if (tokenizer.CurrentTokenType != TokenType.INT_CONST)
        {
            throw new InvalidOperationException(
                $"Expected integer constant, but got {Describe(tokenizer)}");
        }

        int value = tokenizer.IntVal();
        tokenizer.Advance();
        return value;
    }

    public static string ExpectString(IJackTokenizer tokenizer)
    {
        if (tokenizer.CurrentTokenType != TokenType.STRING_CONST)
        {
            throw new InvalidOperationException(
                $"Expected string constant, but got {Describe(tokenizer)}");
        }

        string value = tokenizer.StringVal();
        tokenizer.Advance();
        return value;
    }

    public static bool IsKeyword(IJackTokenizer tokenizer, Keywords keyword)
    {
        return tokenizer.CurrentTokenType == TokenType.KEYWORD &&
               tokenizer.Keyword() == keyword;
    }

    public static bool IsSymbol(IJackTokenizer tokenizer, char symbol)
    {
        return tokenizer.CurrentTokenType == TokenType.SYMBOL &&
               tokenizer.Symbol() == symbol;
    }

    private static string Describe(IJackTokenizer tokenizer)
    {
        return tokenizer.CurrentTokenType switch
        {
            TokenType.KEYWORD =>
                $"keyword '{tokenizer.Keyword()}'",

            TokenType.SYMBOL =>
                $"symbol '{tokenizer.Symbol()}'",

            TokenType.IDENTIFIER =>
                $"identifier '{tokenizer.Identifier()}'",

            TokenType.INT_CONST =>
                $"integer '{tokenizer.IntVal()}'",

            TokenType.STRING_CONST =>
                $"string \"{tokenizer.StringVal()}\"",

            _ => "unknown token"
        };
    }

    public static string CompileType(IJackTokenizer tokenizer)
    {
        if (tokenizer.CurrentTokenType == TokenType.KEYWORD)
        {
            Keywords kw = tokenizer.Keyword();

            if (kw == Keywords.INT ||
                kw == Keywords.CHAR ||
                kw == Keywords.BOOLEAN)
            {
                ExpectKeyword(tokenizer);
                return kw.ToString().ToLower();
            }
        }

        return ExpectIdentifier(tokenizer);
    }

    public static VMSegment KindToSegment(SymbolKind kind)
    {
        return kind switch
        {
            SymbolKind.STATIC => VMSegment.STATIC,
            SymbolKind.FIELD => VMSegment.THIS,
            SymbolKind.ARG => VMSegment.ARG,
            SymbolKind.VAR => VMSegment.LOCAL,
            _ => throw new InvalidOperationException("Invalid symbol kind")
        };
    }

    public static bool IsOperator(char c)
    {
        return c switch
        {
            '+' or '-' or '*' or '/' or
            '&' or '|' or '<' or '>' or '=' => true,
            _ => false
        };
    }

    public static void WriteOperator(IVMWriter vmWriter, char op)
    {
        switch (op)
        {
            case '+':
                vmWriter.WriteArithmetic(VMArithmetic.ADD);
                break;

            case '-':
                vmWriter.WriteArithmetic(VMArithmetic.SUB);
                break;

            case '&':
                vmWriter.WriteArithmetic(VMArithmetic.AND);
                break;

            case '|':
                vmWriter.WriteArithmetic(VMArithmetic.OR);
                break;

            case '<':
                vmWriter.WriteArithmetic(VMArithmetic.LT);
                break;

            case '>':
                vmWriter.WriteArithmetic(VMArithmetic.GT);
                break;

            case '=':
                vmWriter.WriteArithmetic(VMArithmetic.EQ);
                break;

            case '*':
                vmWriter.WriteCall("Math.multiply", 2);
                break;

            case '/':
                vmWriter.WriteCall("Math.divide", 2);
                break;

            default:
                throw new InvalidOperationException($"Unknown operator '{op}'");
        }
    }

}
