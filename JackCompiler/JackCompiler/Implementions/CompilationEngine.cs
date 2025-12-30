using JackCompiler.Abstractions;
using JackCompiler.Enums;

namespace JackCompiler.Implementions
{
    public class CompilationEngine : ICompilationEngine
    {
        private readonly IJackTokenizer tokenizer;
        private readonly ISymbolTable symbolTable;
        private readonly IVMWriter vmWriter;

        private string className;
        private int ifLabelIndex;
        private int whileLabelIndex;

        public CompilationEngine(IJackTokenizer tokenizer,
                                 ISymbolTable symbolTable,
                                 IVMWriter vmWriter)
        {
            this.tokenizer = tokenizer;
            this.symbolTable = symbolTable;
            this.vmWriter = vmWriter;
            ifLabelIndex = 0;
            whileLabelIndex = 0;
        }

        public void CompileClass()
        {
            Helper.ExpectKeyword(tokenizer, Keywords.CLASS);

            className = Helper.ExpectIdentifier(tokenizer);

            Helper.ExpectSymbol(tokenizer, '{');

            // classVarDec*
            while (Helper.IsKeyword(tokenizer, Keywords.STATIC) ||
                   Helper.IsKeyword(tokenizer, Keywords.FIELD))
            {
                CompileClassVarDec();
            }

            // subroutineDec*
            while (Helper.IsKeyword(tokenizer, Keywords.CONSTRUCTOR) ||
                   Helper.IsKeyword(tokenizer, Keywords.FUNCTION) ||
                   Helper.IsKeyword(tokenizer, Keywords.METHOD))
            {
                CompileSubroutine();
            }

            // '}'
            Helper.ExpectSymbol(tokenizer, '}');
        }

        public void CompileClassVarDec()
        {
            // 'static' | 'field'
            Keywords kindKeyword = Helper.ExpectKeyword(tokenizer);

            SymbolKind kind = kindKeyword switch
            {
                Keywords.STATIC => SymbolKind.STATIC,
                Keywords.FIELD => SymbolKind.FIELD,
                _ => throw new InvalidOperationException(
                    $"Invalid class variable kind: {kindKeyword}")
            };

            // type
            string type = Helper.CompileType(tokenizer);

            // varName
            string name = Helper.ExpectIdentifier(tokenizer);
            symbolTable.Define(name, type, kind);

            // (',' varName)*
            while (Helper.IsSymbol(tokenizer, ','))
            {
                Helper.ExpectSymbol(tokenizer, ',');
                name = Helper.ExpectIdentifier(tokenizer);
                symbolTable.Define(name, type, kind);
            }

            // ';'
            Helper.ExpectSymbol(tokenizer, ';');
        }

        public void CompileSubroutine()
        {
            // Reset subroutine-level symbol table
            symbolTable.StartSubroutine();

            // ('constructor' | 'function' | 'method')
            Keywords subroutineKind = Helper.ExpectKeyword(tokenizer);

            // ('void' | type)
            if (Helper.IsKeyword(tokenizer, Keywords.VOID))
            {
                Helper.ExpectKeyword(tokenizer, Keywords.VOID);
            }
            else
            {
                Helper.CompileType(tokenizer);
            }

            // subroutineName
            string subroutineName = Helper.ExpectIdentifier(tokenizer);

            // '('
            Helper.ExpectSymbol(tokenizer, '(');

            // parameterList
            CompileParameterList();

            // ')'
            Helper.ExpectSymbol(tokenizer, ')');

            // subroutineBody
            CompileSubroutineBody(subroutineKind, subroutineName);
        }

        public void CompileParameterList()
        {
            // Empty parameter list
            if (tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                tokenizer.Symbol() == ')')
            {
                return;
            }

            // type
            string type = Helper.CompileType(tokenizer);

            // varName
            string name = Helper.ExpectIdentifier(tokenizer);
            symbolTable.Define(name, type, SymbolKind.ARG);

            // (',' type varName)*
            while (Helper.IsSymbol(tokenizer, ','))
            {
                Helper.ExpectSymbol(tokenizer, ',');

                type = Helper.CompileType(tokenizer);
                name = Helper.ExpectIdentifier(tokenizer);
                symbolTable.Define(name, type, SymbolKind.ARG);
            }
        }

        public void CompileSubroutineBody(Keywords subroutineKind, string subroutineName)
        {
            // '{'
            Helper.ExpectSymbol(tokenizer, '{');

            // If this is a method, define implicit 'this' argument
            if (subroutineKind == Keywords.METHOD)
            {
                symbolTable.Define("this", className, SymbolKind.ARG);
            }

            // varDec*
            while (Helper.IsKeyword(tokenizer, Keywords.VAR))
            {
                CompileVarDec();
            }

            // Emit function declaration
            int localCount = symbolTable.VarCount(SymbolKind.VAR);
            vmWriter.WriteFunction($"{className}.{subroutineName}", localCount);

            // Constructor setup
            if (subroutineKind == Keywords.CONSTRUCTOR)
            {
                int fieldCount = symbolTable.VarCount(SymbolKind.FIELD);
                vmWriter.WritePush(VMSegment.CONST, fieldCount);
                vmWriter.WriteCall("Memory.alloc", 1);
                vmWriter.WritePop(VMSegment.POINTER, 0);
            }

            // Method setup
            else if (subroutineKind == Keywords.METHOD)
            {
                vmWriter.WritePush(VMSegment.ARG, 0);
                vmWriter.WritePop(VMSegment.POINTER, 0);
            }

            // statements
            CompileStatements();

            // '}'
            Helper.ExpectSymbol(tokenizer, '}');
        }

        public void CompileVarDec()
        {
            // 'var'
            Helper.ExpectKeyword(tokenizer, Keywords.VAR);

            // type
            string type = Helper.CompileType(tokenizer);

            // varName
            string name = Helper.ExpectIdentifier(tokenizer);
            symbolTable.Define(name, type, SymbolKind.VAR);

            // (',' varName)*
            while (Helper.IsSymbol(tokenizer, ','))
            {
                Helper.ExpectSymbol(tokenizer, ',');
                name = Helper.ExpectIdentifier(tokenizer);
                symbolTable.Define(name, type, SymbolKind.VAR);
            }

            // ';'
            Helper.ExpectSymbol(tokenizer, ';');
        }

        public void CompileStatements()
        {
            while (tokenizer.CurrentTokenType == TokenType.KEYWORD)
            {
                switch (tokenizer.Keyword())
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
                        return;
                }
            }
        }

        public void CompileLet()
        {
            // 'let'
            Helper.ExpectKeyword(tokenizer, Keywords.LET);

            // varName
            string varName = Helper.ExpectIdentifier(tokenizer);

            SymbolKind kind = symbolTable.KindOf(varName);
            int index = symbolTable.IndexOf(varName);

            if (kind == SymbolKind.NONE)
                throw new InvalidOperationException($"Undefined variable '{varName}'");

            bool isArray = false;

            // ('[' expression ']')?
            if (Helper.IsSymbol(tokenizer, '['))
            {
                isArray = true;

                // push base address
                vmWriter.WritePush(Helper.KindToSegment(kind), index);

                Helper.ExpectSymbol(tokenizer, '[');

                // compile index expression (will handle pushing address)
                CompileExpression();

                Helper.ExpectSymbol(tokenizer, ']');

                // base + index
                vmWriter.WriteArithmetic(VMArithmetic.ADD);
            }

            // '='
            Helper.ExpectSymbol(tokenizer, '=');

            // RHS expression
            CompileExpression();

            // ';'
            Helper.ExpectSymbol(tokenizer, ';');

            if (isArray)
            {
                // arr[i] = value
                // stack: [address, value]

                vmWriter.WritePop(VMSegment.TEMP, 0);     // store value
                vmWriter.WritePop(VMSegment.POINTER, 1);  // THAT = address
                vmWriter.WritePush(VMSegment.TEMP, 0);    // restore value
                vmWriter.WritePop(VMSegment.THAT, 0);     // *THAT = value
            }
            else
            {
                // simple assignment
                vmWriter.WritePop(Helper.KindToSegment(kind), index);
            }
        }

        public void CompileExpression()
        {
            // first term
            CompileTerm();

            // (op term)*
            while (tokenizer.CurrentTokenType == TokenType.SYMBOL &&
                   Helper.IsOperator(tokenizer.Symbol()))
            {
                char op = tokenizer.Symbol();
                Helper.ExpectSymbol(tokenizer, op);

                CompileTerm();

                Helper.WriteOperator(vmWriter, op);
            }
        }

        public void CompileTerm()
        {
            switch (tokenizer.CurrentTokenType)
            {
                case TokenType.INT_CONST: //1
                    vmWriter.WritePush(VMSegment.CONST, tokenizer.IntVal());
                    tokenizer.Advance();
                    break;

                case TokenType.STRING_CONST: //"Plain Text"
                    CompileStringConstant();
                    break;

                case TokenType.KEYWORD: //true, false, etc
                    CompileKeywordConstant();
                    break;

                case TokenType.IDENTIFIER: //varName
                    CompileIdentifierTerm();
                    break;

                case TokenType.SYMBOL:
                    if (tokenizer.Symbol() == '(')
                    {
                        Helper.ExpectSymbol(tokenizer, '(');
                        CompileExpression();
                        Helper.ExpectSymbol(tokenizer, ')');
                    }
                    else if (tokenizer.Symbol() == '-' || tokenizer.Symbol() == '~')
                    {
                        char unaryOp = tokenizer.Symbol();
                        Helper.ExpectSymbol(tokenizer, unaryOp);
                        CompileTerm();

                        if (unaryOp == '-')
                            vmWriter.WriteArithmetic(VMArithmetic.NEG);
                        else
                            vmWriter.WriteArithmetic(VMArithmetic.NOT);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid symbol in term");
                    }
                    break;

                default:
                    throw new InvalidOperationException("Invalid term");
            }
        }

        public void CompileStringConstant()
        {
            string value = tokenizer.StringVal();
            tokenizer.Advance();

            vmWriter.WritePush(VMSegment.CONST, value.Length);
            vmWriter.WriteCall("String.new", 1);

            foreach (char c in value)
            {
                vmWriter.WritePush(VMSegment.CONST, c);
                vmWriter.WriteCall("String.appendChar", 2);
            }
        }

        public void CompileKeywordConstant()
        {
            switch (tokenizer.Keyword())
            {
                case Keywords.TRUE:
                    vmWriter.WritePush(VMSegment.CONST, 0);
                    vmWriter.WriteArithmetic(VMArithmetic.NOT);
                    break;

                case Keywords.FALSE:
                case Keywords.NULL:
                    vmWriter.WritePush(VMSegment.CONST, 0);
                    break;

                case Keywords.THIS:
                    vmWriter.WritePush(VMSegment.POINTER, 0);
                    break;

                default:
                    throw new InvalidOperationException("Invalid keyword constant");
            }

            tokenizer.Advance();
        }

        public void CompileIdentifierTerm()
        {
            string name = Helper.ExpectIdentifier(tokenizer);

            // varName '[' expression ']'
            if (Helper.IsSymbol(tokenizer, '['))
            {
                SymbolKind kind = symbolTable.KindOf(name); //local, arg, this, etc
                int index = symbolTable.IndexOf(name); //at any index?

                vmWriter.WritePush(Helper.KindToSegment(kind), index); //push the base address of the array

                Helper.ExpectSymbol(tokenizer, '[');
                CompileExpression();//stack: [base address, indexValue]
                Helper.ExpectSymbol(tokenizer, ']');

                vmWriter.WriteArithmetic(VMArithmetic.ADD);
                vmWriter.WritePop(VMSegment.POINTER, 1);
                vmWriter.WritePush(VMSegment.THAT, 0); //arr[i]
            }
            // subroutineCall
            else if (Helper.IsSymbol(tokenizer, '(') || Helper.IsSymbol(tokenizer, '.'))
            {
                CompileSubroutineCallWithName(name);
            }
            // simple varName
            else
            {
                SymbolKind kind = symbolTable.KindOf(name);
                int index = symbolTable.IndexOf(name);

                vmWriter.WritePush(Helper.KindToSegment(kind), index);
            }
        }

        public void CompileSubroutineCallWithName(string name)
        {
            int argCount = 0;
            string fullName;

            if (Helper.IsSymbol(tokenizer, '.'))
            {
                Helper.ExpectSymbol(tokenizer, '.');
                string subroutineName = Helper.ExpectIdentifier(tokenizer);

                SymbolKind kind = symbolTable.KindOf(name);

                if (kind != SymbolKind.NONE)//object method
                {
                    // method call on object (push this )
                    vmWriter.WritePush(
                        Helper.KindToSegment(kind),
                        symbolTable.IndexOf(name)); //push the object address onto the stack

                    fullName = $"{symbolTable.TypeOf(name)}.{subroutineName}"; //object.method()
                    argCount = 1;
                }
                else
                {
                    // function call
                    fullName = $"{name}.{subroutineName}"; //class.function()
                }
            }
            else
            {
                // method call in same class
                vmWriter.WritePush(VMSegment.POINTER, 0); //function()
                fullName = $"{className}.{name}";
                argCount = 1;
            }

            Helper.ExpectSymbol(tokenizer, '(');
            argCount += CompileExpressionList();
            Helper.ExpectSymbol(tokenizer, ')');

            vmWriter.WriteCall(fullName, argCount);
        }

        public void CompileIf()
        {
            int labelIndex = ifLabelIndex++;

            string trueLabel = $"IF_TRUE{labelIndex}";
            string falseLabel = $"IF_FALSE{labelIndex}";
            string endLabel = $"IF_END{labelIndex}";

            // 'if'
            Helper.ExpectKeyword(tokenizer, Keywords.IF);

            // '('
            Helper.ExpectSymbol(tokenizer, '(');

            // condition expression
            CompileExpression(); //evaluate the exp then push it into the stack

            // ')'
            Helper.ExpectSymbol(tokenizer, ')');

            // if-goto TRUE
            vmWriter.WriteIf(trueLabel);

            // goto FALSE
            vmWriter.WriteGoto(falseLabel);

            // TRUE label
            vmWriter.WriteLabel(trueLabel);

            // '{'
            Helper.ExpectSymbol(tokenizer, '{');

            // true statements
            CompileStatements();

            // '}'
            Helper.ExpectSymbol(tokenizer, '}');

            // optional else
            if (tokenizer.CurrentTokenType == TokenType.KEYWORD &&
                tokenizer.Keyword() == Keywords.ELSE)
            {
                // jump to end after true block
                vmWriter.WriteGoto(endLabel);

                // FALSE label
                vmWriter.WriteLabel(falseLabel);

                // 'else'
                Helper.ExpectKeyword(tokenizer, Keywords.ELSE);

                // '{'
                Helper.ExpectSymbol(tokenizer, '{');

                // else statements
                CompileStatements();

                // '}'
                Helper.ExpectSymbol(tokenizer, '}');

                // END label
                vmWriter.WriteLabel(endLabel);
            }
            else
            {
                // no else → false jumps directly here
                vmWriter.WriteLabel(falseLabel);
            }
        }

        public void CompileWhile()
        {
            int labelIndex = whileLabelIndex++;

            string expLabel = $"WHILE_EXP{labelIndex}";
            string endLabel = $"WHILE_END{labelIndex}";

            // 'while'
            Helper.ExpectKeyword(tokenizer, Keywords.WHILE);

            // label WHILE_EXP
            vmWriter.WriteLabel(expLabel);

            // '('
            Helper.ExpectSymbol(tokenizer, '(');

            // condition expression
            CompileExpression();

            // ')'
            Helper.ExpectSymbol(tokenizer, ')');

            // if condition is false → jump to end
            vmWriter.WriteArithmetic(VMArithmetic.NOT);
            vmWriter.WriteIf(endLabel);

            // '{'
            Helper.ExpectSymbol(tokenizer, '{');

            // loop body
            CompileStatements();

            // '}'
            Helper.ExpectSymbol(tokenizer, '}');

            // go back to condition
            vmWriter.WriteGoto(expLabel);

            // end label
            vmWriter.WriteLabel(endLabel);
        }

        public void CompileDo()
        {
            // 'do'
            Helper.ExpectKeyword(tokenizer, Keywords.DO);

            // subroutineCall (leaves a return value on stack)
            CompileSubroutineCall();

            // ';'
            Helper.ExpectSymbol(tokenizer, ';');

            // discard return value
            vmWriter.WritePop(VMSegment.TEMP, 0);
        }

        public void CompileSubroutineCall()
        {
            // read the first identifier
            string name = Helper.ExpectIdentifier(tokenizer);

            // delegate the real work
            CompileSubroutineCallWithName(name);
        }

        public void CompileReturn()
        {
            // 'return'
            Helper.ExpectKeyword(tokenizer, Keywords.RETURN);

            // return expression?
            if (!Helper.IsSymbol(tokenizer, ';'))
            {
                CompileExpression();   // leaves value on stack
            }
            else
            {
                // void return → push dummy value
                vmWriter.WritePush(VMSegment.CONST, 0);
            }

            // ';'
            Helper.ExpectSymbol(tokenizer, ';');

            // VM return
            vmWriter.WriteReturn();
        }

        public int CompileExpressionList()
        {
            int count = 0;

            // empty list → next token is ')'
            if (Helper.IsSymbol(tokenizer, ')'))
                return 0;

            // first expression
            CompileExpression();
            count++;

            // (',' expression)*
            while (Helper.IsSymbol(tokenizer, ','))
            {
                Helper.ExpectSymbol(tokenizer, ',');
                CompileExpression();
                count++;
            }

            return count;
        }

    }
}
