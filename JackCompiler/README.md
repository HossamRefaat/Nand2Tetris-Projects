# Jack Compiler

A compiler for the Jack programming language that translates `.jack` source files into Virtual Machine (VM) code (`.vm` files).

## What It Does

Compiles Jack source code into VM instructions that can be executed by the VM Translator. Processes single files or entire directories of `.jack` files.

## Usage

```bash
dotnet run --project JackCompiler/JackCompiler.csproj -- <input_path>
```

**Examples:**
- Single file: `dotnet run --project JackCompiler/JackCompiler.csproj -- Main.jack`
- Directory: `dotnet run --project JackCompiler/JackCompiler.csproj -- ./src`

## Architecture

- **JackTokenizer**: Tokenizes Jack source code into lexical tokens
- **CompilationEngine**: Parses tokens and generates VM code using recursive descent
- **SymbolTable**: Manages class-level and subroutine-level symbol scoping
- **VMWriter**: Writes VM commands to output files

