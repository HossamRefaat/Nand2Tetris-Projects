using SyntaxAnalyzer.Abstractions;
using SyntaxAnalyzer.Enums;

namespace SyntaxAnalyzer.Implementions
{
    public class CompilationEngine : ICompilationEngine, IDisposable
    {
        private readonly IJackTokenizer _tokenizer;
        private readonly XmlOutputWriter _xml;

        public CompilationEngine(IJackTokenizer jackTokenizer, string outPath)
        {
            _tokenizer = jackTokenizer;
            _xml = new XmlOutputWriter(outPath);
        }

        public void CompileClass()
        {
            _xml.OpenTag("class");

            // 'class'
            _tokenizer.Advance();
            ExpectKeyword(Keywords.CLASS);
            WriteCurrentToken();

            // className
            _tokenizer.Advance();
            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();

            // '{'
            _tokenizer.Advance();
            ExpectSymbol('{');
            WriteCurrentToken();

            // classVarDec*
            _tokenizer.Advance();
            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD &&
                  (_tokenizer.Keyword() == Keywords.STATIC ||
                   _tokenizer.Keyword() == Keywords.FIELD))
            {
                CompileClassVarDec();
                _tokenizer.Advance();
            }

            // subroutineDec*
            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD &&
                  (_tokenizer.Keyword() == Keywords.CONSTRUCTOR ||
                   _tokenizer.Keyword() == Keywords.FUNCTION ||
                   _tokenizer.Keyword() == Keywords.METHOD))
            {
                CompileSubroutine();
                _tokenizer.Advance();
            }

            // '}'
            ExpectSymbol('}');
            WriteCurrentToken();

            _xml.CloseTag("class");
        }

        public void CompileClassVarDec()
        {
            _xml.OpenTag("classVarDec");

            // ('static' | 'field')
            Expect(TokenType.KEYWORD);
            WriteCurrentToken();   // static | field

            // type
            _tokenizer.Advance();
            if (_tokenizer.CurrentTokenType == TokenType.KEYWORD ||
                _tokenizer.CurrentTokenType == TokenType.IDENTIFIER)
            {
                // KEYWORD: int, char, boolean, void
                // IDENTIFIER: user-defined class (assume valid)

                WriteCurrentToken();
            }
            else
            {
                throw new InvalidOperationException("Expected type in classVarDec.");
            }

            // varName
            _tokenizer.Advance();
            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();

            // (',' varName)*
            _tokenizer.Advance();
            while (_tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                   _tokenizer.Symbol() == ',')
            {
                WriteCurrentToken();   // ,

                _tokenizer.Advance();
                Expect(TokenType.IDENTIFIER);
                WriteCurrentToken();   // varName

                _tokenizer.Advance();
            }

            // ';'
            ExpectSymbol(';');
            WriteCurrentToken();

            _xml.CloseTag("classVarDec");
        }

        public void CompileSubroutine()
        {
            _xml.OpenTag("subroutineDec");

            // 1. ('constructor' | 'function' | 'method')
            Expect(TokenType.KEYWORD);
            var kw = _tokenizer.Keyword();
            if (kw != Keywords.CONSTRUCTOR && kw != Keywords.FUNCTION && kw != Keywords.METHOD)
                throw new InvalidOperationException("Expected constructor, function, or method");
            WriteCurrentToken();

            // 2. Return type ('void' | type)
            _tokenizer.Advance();
            if (_tokenizer.CurrentTokenType == TokenType.KEYWORD ||
                _tokenizer.CurrentTokenType == TokenType.IDENTIFIER)
            {
                // KEYWORD: int, char, boolean, void
                // IDENTIFIER: user-defined class (assume valid)
                WriteCurrentToken();
            }
            else
            {
                throw new InvalidOperationException("Expected return type (void, built-in, or class name)");
            }

            // 3. subroutineName
            _tokenizer.Advance();
            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();

            // 4. '('
            _tokenizer.Advance();
            ExpectSymbol('(');
            WriteCurrentToken();

            // 5. parameterList
            _tokenizer.Advance();
            CompileParameterList();

            // 6. ')'
            ExpectSymbol(')');
            WriteCurrentToken();

            // 7. subroutineBody
            _tokenizer.Advance();
            ExpectSymbol('{');
            _xml.OpenTag("subroutineBody");
            WriteCurrentToken(); // {

            _tokenizer.Advance();
            // Parse local variable declarations (varDec*)
            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD &&
                   _tokenizer.Keyword() == Keywords.VAR)
            {
                CompileVarDec();
                _tokenizer.Advance();
            }

            // Parse statements
            CompileStatements();

            // Closing '}'
            ExpectSymbol('}');
            WriteCurrentToken();
            _xml.CloseTag("subroutineBody");

            _xml.CloseTag("subroutineDec");
        }

        public void CompileParameterList()
        {
            _xml.OpenTag("parameterList");

            // parameterList can be empty
            if (_tokenizer.CurrentTokenType != TokenType.SYMBOL || _tokenizer.Symbol() != ')')
            {
                while (true)
                {
                    // KEYWORD: int, char, boolean, void
                    // IDENTIFIER: user-defined class (assume valid)
                    if (_tokenizer.CurrentTokenType == TokenType.KEYWORD ||
                        _tokenizer.CurrentTokenType == TokenType.IDENTIFIER)
                    {
                        WriteCurrentToken(); // type
                    }
                    else
                    {
                        throw new InvalidOperationException("Expected type in parameterList");
                    }

                    // varName (IDENTIFIER)
                    _tokenizer.Advance();
                    Expect(TokenType.IDENTIFIER);
                    WriteCurrentToken();

                    // Check for comma
                    _tokenizer.Advance();
                    if (_tokenizer.CurrentTokenType == TokenType.SYMBOL && _tokenizer.Symbol() == ',')
                    {
                        WriteCurrentToken();
                        _tokenizer.Advance();
                    }
                    else
                    {
                        break; // no more parameters
                    }
                }
            }

            _xml.CloseTag("parameterList");
        }

        public void CompileSubroutineBody()
        {
            _xml.OpenTag("subroutineBody");

            // Expect '{'
            ExpectSymbol('{');
            WriteCurrentToken();
            _tokenizer.Advance();

            // Parse local variable declarations (varDec*)
            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD &&
                   _tokenizer.Keyword() == Keywords.VAR)
            {
                CompileVarDec();
                _tokenizer.Advance();
            }

            // Parse statements
            _xml.OpenTag("statements");
            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD)
            {
                var kw = _tokenizer.Keyword();
                switch (kw)
                {
                    case Keywords.LET:
                        CompileLet();
                        break;
                    case Keywords.IF:
                        CompileIf();
                        break;
                    case Keywords.WHILE:
                        CompileWhile();
                        break;
                    case Keywords.DO:
                        CompileDo();
                        break;
                    case Keywords.RETURN:
                        CompileReturn();
                        break;
                    default:
                        // Not a statement keyword, stop
                        goto EndStatements;
                }
            }
        EndStatements:
            _xml.CloseTag("statements");

            // Expect '}'
            ExpectSymbol('}');
            WriteCurrentToken();

            _xml.CloseTag("subroutineBody");
        }

        public void CompileVarDec()
        {
            _xml.OpenTag("varDec");

            // Expect 'var' keyword
            ExpectKeyword(Keywords.VAR);
            WriteCurrentToken();

            // Advance to type
            _tokenizer.Advance();
            if (_tokenizer.CurrentTokenType == TokenType.KEYWORD ||
                _tokenizer.CurrentTokenType == TokenType.IDENTIFIER)
            {
                // KEYWORD: int, char, boolean, void
                // IDENTIFIER: user-defined class (assume valid)

                WriteCurrentToken();
            }
            else
            {
                throw new InvalidOperationException("Expected type in var declaration");
            }

            // Advance to first varName
            _tokenizer.Advance();
            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();

            // Advance and handle optional comma-separated variables
            _tokenizer.Advance();
            while (_tokenizer.CurrentTokenType == TokenType.SYMBOL && _tokenizer.Symbol() == ',')
            {
                WriteCurrentToken(); // comma
                _tokenizer.Advance();

                Expect(TokenType.IDENTIFIER);
                WriteCurrentToken();

                _tokenizer.Advance();
            }

            // Expect semicolon ';'
            ExpectSymbol(';');
            WriteCurrentToken();

            _xml.CloseTag("varDec");
        }

        public void CompileStatements()
        {
            _xml.OpenTag("statements");

            while (_tokenizer.CurrentTokenType == TokenType.KEYWORD)
            {
                var kw = _tokenizer.Keyword();

                switch (kw)
                {
                    case Keywords.LET:
                        CompileLet();
                        break;
                    case Keywords.IF:
                        CompileIf();
                        break;
                    case Keywords.WHILE:
                        CompileWhile();
                        break;
                    case Keywords.DO:
                        CompileDo();
                        break;
                    case Keywords.RETURN:
                        CompileReturn();
                        break;
                    default:
                        // Not a statement keyword, stop parsing statements
                        goto EndStatements;
                }
            }
        EndStatements:
            _xml.CloseTag("statements");
        }

        public void CompileDo()
        {
            _xml.OpenTag("doStatement");

            ExpectKeyword(Keywords.DO);
            WriteCurrentToken();
            _tokenizer.Advance();

            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();
            _tokenizer.Advance();

            CompileSubroutineCallAfterIdentifier();

            ExpectSymbol(';');
            WriteCurrentToken();
            _tokenizer.Advance();

            _xml.CloseTag("doStatement");
        }

        public void CompileWhile()
        {
            _xml.OpenTag("whileStatement");

            // 'while'
            ExpectKeyword(Keywords.WHILE);
            WriteCurrentToken();

            // '('
            _tokenizer.Advance();
            ExpectSymbol('(');
            WriteCurrentToken();

            // expression
            _tokenizer.Advance();
            CompileExpression();

            // ')'
            ExpectSymbol(')');
            WriteCurrentToken();

            // '{'
            _tokenizer.Advance();
            ExpectSymbol('{');
            WriteCurrentToken();

            // statements inside while
            _tokenizer.Advance();
            CompileStatements();

            // '}'
            ExpectSymbol('}');
            WriteCurrentToken();

            _tokenizer.Advance();

            _xml.CloseTag("whileStatement");
        }

        public void CompileReturn()
        {
            _xml.OpenTag("returnStatement");

            // 'return' keyword
            ExpectKeyword(Keywords.RETURN);
            WriteCurrentToken();

            // Advance to check if there is an expression
            _tokenizer.Advance();
            if (!(_tokenizer.CurrentTokenType == TokenType.SYMBOL && _tokenizer.Symbol() == ';'))
            {
                // There is an expression
                CompileExpression();
            }

            // Expect semicolon ';'
            ExpectSymbol(';');
            WriteCurrentToken();
            _tokenizer.Advance();

            _xml.CloseTag("returnStatement");
        }

        public void CompileIf()
        {
            _xml.OpenTag("ifStatement");

            // 'if' keyword
            ExpectKeyword(Keywords.IF);
            WriteCurrentToken();

            // '('
            _tokenizer.Advance();
            ExpectSymbol('(');
            WriteCurrentToken();

            // expression inside parentheses
            _tokenizer.Advance();
            CompileExpression();

            // ')'
            ExpectSymbol(')');
            WriteCurrentToken();

            // '{' statements '}'
            _tokenizer.Advance();
            ExpectSymbol('{');
            WriteCurrentToken();

            _tokenizer.Advance();
            CompileStatements();

            ExpectSymbol('}');
            WriteCurrentToken();

            // Optional 'else' block
            _tokenizer.Advance();
            if (_tokenizer.CurrentTokenType == TokenType.KEYWORD && _tokenizer.Keyword() == Keywords.ELSE)
            {
                WriteCurrentToken(); // 'else'

                _tokenizer.Advance();
                ExpectSymbol('{');
                WriteCurrentToken();

                _tokenizer.Advance();
                CompileStatements();

                ExpectSymbol('}');
                WriteCurrentToken();
                _tokenizer.Advance();
            }

            _xml.CloseTag("ifStatement");
        }

        public void CompileLet()
        {
            _xml.OpenTag("letStatement");

            // 'let' keyword
            ExpectKeyword(Keywords.LET);
            WriteCurrentToken();

            // varName
            _tokenizer.Advance();
            Expect(TokenType.IDENTIFIER);
            WriteCurrentToken();

            // Optional array indexing [expression]
            _tokenizer.Advance();
            if (_tokenizer.CurrentTokenType == TokenType.SYMBOL && _tokenizer.Symbol() == '[')
            {
                WriteCurrentToken(); // '['
                _tokenizer.Advance();

                CompileExpression();

                ExpectSymbol(']');
                WriteCurrentToken();
                _tokenizer.Advance();
            }

            // '=' symbol
            ExpectSymbol('=');
            WriteCurrentToken();

            // expression
            _tokenizer.Advance();
            CompileExpression();

            // ';' symbol
            ExpectSymbol(';');
            WriteCurrentToken();

            _xml.CloseTag("letStatement");

            // Move to next token
            _tokenizer.Advance();
        }

        public void CompileTerm()
        {
            _xml.OpenTag("term");

            // integerConstant | stringConstant
            if (_tokenizer.CurrentTokenType == TokenType.INT_CONST ||
                _tokenizer.CurrentTokenType == TokenType.STRING_CONST)
            {
                WriteCurrentToken();
                _tokenizer.Advance();
            }

            // keywordConstant: true | false | null | this
            else if (_tokenizer.CurrentTokenType == TokenType.KEYWORD)
            {
                var kw = _tokenizer.Keyword();
                if (kw != Keywords.TRUE &&
                    kw != Keywords.FALSE &&
                    kw != Keywords.NULL &&
                    kw != Keywords.THIS)
                {
                    throw new InvalidOperationException("Invalid keyword in term");
                }

                WriteCurrentToken();
                _tokenizer.Advance();
            }

            // identifier: varName | varName[expression] | subroutineCall
            else if (_tokenizer.CurrentTokenType == TokenType.IDENTIFIER)
            {
                WriteCurrentToken(); // identifier
                _tokenizer.Advance();

                if (_tokenizer.CurrentTokenType == TokenType.SYMBOL)
                {
                    if (_tokenizer.Symbol() == '[')
                    {
                        WriteCurrentToken();
                        _tokenizer.Advance();
                        CompileExpression();
                        ExpectSymbol(']');
                        WriteCurrentToken();
                        _tokenizer.Advance();
                    }
                    else if (_tokenizer.Symbol() == '(' || _tokenizer.Symbol() == '.')
                    {
                        CompileSubroutineCallAfterIdentifier();
                    }
                }
            }

            // '(' expression ')' | unaryOp term
            else if (_tokenizer.CurrentTokenType == TokenType.SYMBOL)
            {
                // '(' expression ')'
                if (_tokenizer.Symbol() == '(')
                {
                    WriteCurrentToken();
                    _tokenizer.Advance();

                    CompileExpression();

                    ExpectSymbol(')');
                    WriteCurrentToken();
                    _tokenizer.Advance();
                }
                // unaryOp term
                else if (_tokenizer.Symbol() == '-' || _tokenizer.Symbol() == '~')
                {
                    //what is the unaryOp term and why we used recursion here
                    WriteCurrentToken();
                    _tokenizer.Advance();
                    CompileTerm();
                }
                else
                {
                    throw new InvalidOperationException("Invalid symbol in term");
                }
            }

            else
            {
                throw new InvalidOperationException("Invalid term");
            }

            _xml.CloseTag("term");
        }

        public void CompileExpression()
        {
            _xml.OpenTag("expression");

            // first term
            CompileTerm();

            // (op term)*
            while (_tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                   IsOperator(_tokenizer.Symbol()))
            {
                WriteCurrentToken();   // operator
                _tokenizer.Advance();

                CompileTerm();
            }

            _xml.CloseTag("expression");
        }

        public void CompileExpressionList()
        {
            _xml.OpenTag("expressionList");

            // Empty expression list: next token is ')'
            if (_tokenizer.CurrentTokenType != TokenType.SYMBOL ||
                _tokenizer.Symbol() != ')')
            {
                // First expression
                CompileExpression();

                // (',' expression)*
                while (_tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                       _tokenizer.Symbol() == ',')
                {
                    WriteCurrentToken(); // ','
                    _tokenizer.Advance();
                    CompileExpression();
                }
            }

            _xml.CloseTag("expressionList");
        }



        #region Helper Methods
        private void Expect(TokenType type)
        {
            if (_tokenizer.CurrentTokenType != type)
                throw new InvalidOperationException($"Expected {type}");
        }

        private void ExpectKeyword(Keywords keyword)
        {
            if (_tokenizer.CurrentTokenType != TokenType.KEYWORD ||
                _tokenizer.Keyword() != keyword)
                throw new InvalidOperationException($"Expected keyword '{keyword.ToString().ToLower()}'");
        }

        private void ExpectSymbol(char symbol)
        {
            if (_tokenizer.CurrentTokenType != TokenType.SYMBOL ||
                _tokenizer.Symbol() != symbol)
                throw new InvalidOperationException($"Expected symbol '{symbol}'");
        }

        private void WriteCurrentToken()
        {
            switch (_tokenizer.CurrentTokenType)
            {
                case TokenType.KEYWORD:
                    _xml.WriteToken("keyword", _tokenizer.Keyword().ToString().ToLower());
                    break;
                case TokenType.SYMBOL:
                    _xml.WriteToken("symbol", _tokenizer.Symbol().ToString());
                    break;
                case TokenType.IDENTIFIER:
                    _xml.WriteToken("identifier", _tokenizer.Identifier());
                    break;
                case TokenType.INT_CONST:
                    _xml.WriteToken("integerConstant", _tokenizer.IntVal().ToString());
                    break;
                case TokenType.STRING_CONST:
                    _xml.WriteToken("stringConstant", _tokenizer.StringVal());
                    break;
            }
        }

        private void CompileSubroutineCallAfterIdentifier()
        {
            // optional: .subroutineName
            if (_tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                _tokenizer.Symbol() == '.')
            {
                WriteCurrentToken(); // '.'
                _tokenizer.Advance();

                Expect(TokenType.IDENTIFIER);
                WriteCurrentToken(); // subroutineName
                _tokenizer.Advance();
            }

            // '('
            ExpectSymbol('(');
            WriteCurrentToken();
            _tokenizer.Advance();

            CompileExpressionList();

            // ')'
            ExpectSymbol(')');
            WriteCurrentToken();
            _tokenizer.Advance();
        }

        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' ||
                   c == '&' || c == '|' || c == '<' || c == '>' || c == '=';
        }


        #endregion

        public void Dispose()
        {
            _xml?.Dispose();
        }
    }
}
