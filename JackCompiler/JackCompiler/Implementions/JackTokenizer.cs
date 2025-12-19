using JackCompiler.Abstractions;
using JackCompiler.Enums;
using System.Text.RegularExpressions;

namespace JackCompiler.Implementions
{
    internal class JackTokenizer : IJackTokenizer
    {
        private readonly List<(string Value, TokenType Type)> _tokens;
        private int _currentIndex;
        private static readonly HashSet<string> _keywords = new()
        {
            "class", "constructor", "function", "method",
            "field", "static", "var",
            "int", "char", "boolean", "void",
            "true", "false", "null", "this",
            "let", "do", "if", "else", "while", "return"
        };


        public JackTokenizer(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Jack file not found.", filePath);

            _tokens = new List<(string Value, TokenType Type)>();
            _currentIndex = -1;

            string source = File.ReadAllText(filePath);

            source = RemoveComments(source);
            _tokens = BuildTokens(source);
        }

        public TokenType CurrentTokenType
        {
            get
            {
                if (_currentIndex < 0 || _currentIndex >= _tokens.Count)
                    throw new InvalidOperationException("No current token.");

                return _tokens[_currentIndex].Type;
            }
        }

        public void Advance()
        {
            if (!HasMoreTokens())
                throw new InvalidOperationException("No more tokens.");
            _currentIndex++;
        }

        public bool HasMoreTokens() => _currentIndex + 1 < _tokens.Count;

        public string Identifier()
        {
            if (CurrentTokenType != TokenType.IDENTIFIER)
                throw new InvalidOperationException("Current token is not an identifier.");

            return _tokens[_currentIndex].Value;
        }


        public int IntVal()
        {
            if (CurrentTokenType != TokenType.INT_CONST)
                throw new InvalidOperationException("Current token is not an integer constant.");

            string token = _tokens[_currentIndex].Value;

            if (!int.TryParse(token, out int value))
                throw new InvalidOperationException($"Invalid integer value: {token}");

            if (value < 0 || value > 32767)
                throw new InvalidOperationException($"Out of range");

            return value;
        }


        public Keywords Keyword()
        {
            if (CurrentTokenType != TokenType.KEYWORD)
                throw new InvalidOperationException("Current token is not a keyword.");

            string token = _tokens[_currentIndex].Value;

            return token switch
            {
                "class" => Keywords.CLASS,
                "method" => Keywords.METHOD,
                "function" => Keywords.FUNCTION,
                "constructor" => Keywords.CONSTRUCTOR,
                "int" => Keywords.INT,
                "boolean" => Keywords.BOOLEAN,
                "char" => Keywords.CHAR,
                "void" => Keywords.VOID,
                "var" => Keywords.VAR,
                "static" => Keywords.STATIC,
                "field" => Keywords.FIELD,
                "let" => Keywords.LET,
                "do" => Keywords.DO,
                "if" => Keywords.IF,
                "else" => Keywords.ELSE,
                "while" => Keywords.WHILE,
                "return" => Keywords.RETURN,
                "true" => Keywords.TRUE,
                "false" => Keywords.FALSE,
                "null" => Keywords.NULL,
                "this" => Keywords.THIS,
                _ => throw new InvalidOperationException($"Unknown keyword: {token}")
            };
        }


        public string StringVal()
        {
            if (CurrentTokenType != TokenType.STRING_CONST)
                throw new InvalidOperationException("Current token is not a string constant.");

            return _tokens[_currentIndex].Value;
        }


        public char Symbol()
        {
            if (CurrentTokenType != TokenType.SYMBOL)
                throw new InvalidOperationException("Current token is not a symbol.");

            string token = _tokens[_currentIndex].Value;

            if (token.Length != 1)
                throw new InvalidOperationException("Symbol token is not a single character.");

            return token[0];
        }


        private static string RemoveComments(string input)
        {
            // Multi-line comments /* ... */
            input = Regex.Replace(input, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Single-line comments //
            input = Regex.Replace(input, @"//.*", "");

            return input;
        }

        private static List<(string Value, TokenType Type)> BuildTokens(string input)
        {
            var tokens = new List<(string Value, TokenType Type)>();
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                // skip whitespace
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // string constant
                if (c == '"')
                {
                    i++;
                    int start = i;

                    while (i < input.Length && input[i] != '"')
                    {
                        if (input[i] == '\n')
                            throw new InvalidOperationException("Newline in string constant.");
                        i++;
                    }

                    if (i >= input.Length)
                        throw new InvalidOperationException("Unterminated string constant.");

                    string value = input.Substring(start, i - start);
                    tokens.Add((value, TokenType.STRING_CONST));

                    i++; // skip closing "
                    continue;
                }

                // symbol
                if ("{}()[].,;+-*/&|<>=~".Contains(c))
                {
                    tokens.Add((c.ToString(), TokenType.SYMBOL));
                    i++;
                    continue;
                }

                // integer constant
                if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < input.Length && char.IsDigit(input[i])) i++;

                    string value = input.Substring(start, i - start);
                    tokens.Add((value, TokenType.INT_CONST));
                    continue;
                }

                // identifier or keyword
                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < input.Length &&
                           (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                        i++;

                    string value = input.Substring(start, i - start);

                    TokenType type = _keywords.Contains(value)
                        ? TokenType.KEYWORD
                        : TokenType.IDENTIFIER;

                    tokens.Add((value, type));
                    continue;
                }

                i++;
            }

            return tokens;
        }

        public void StepBack()
        {
            if (_currentIndex > 0) _currentIndex--;
        }
    }
}
