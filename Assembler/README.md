# Hack Assembler

Assembler for the Hack assembly language, built with C# and .NET 8. This assembler translates Hack assembly code into binary machine code for the Hack computer platform.

- ✅ **Command-line Interface**: Easy-to-use terminal interface

## Requirements

- .NET 8.0 or later
- Windows, macOS, or Linux

## Installation

1. Clone or download the project
2. Navigate to the project directory
3. Build the project:
   ```bash
   dotnet build
   ```

## Usage

### Basic Usage
```bash
dotnet run <input_file.asm>
```

### Examples
```bash
# Assemble a program
dotnet run program.asm
# Output: Assembly complete. Output written to 'program_out.asm'

# Assemble from different directory
dotnet run C:\path\to\myprogram.asm
# Output: Assembly complete. Output written to 'C:\path\to\myprogram_out.asm'
```

### Input/Output
- **Input**: Hack assembly files (`.asm` extension recommended)
- **Output**: Binary machine code files with `_out` suffix in the same directory as input

## Supported Assembly Language

### A-Instructions
```assembly
@value    // Load constant or symbol address
@100      // Load constant 100
@LOOP     // Load address of LOOP label
```

### C-Instructions
```assembly
dest=comp;jump

// Examples:
D=A       // Set D register to A register value
M=D+1     // Set memory to D register + 1
D;JGT     // Jump if D > 0
AMD=M-1   // Set A, M, D to M-1
```

### Labels
```assembly
(LOOP)    // Define LOOP label
```

### Comments
```assembly
// This is a comment
@2        // Load constant 2
D=A       // Set D to A
```

## Predefined Symbols

| Symbol | Value | Description |
|--------|-------|-------------|
| R0-R15 | 0-15  | General purpose registers |
| SCREEN | 16384 | Screen memory map base |
| KBD    | 24576 | Keyboard memory map |
| SP     | 0     | Stack pointer |
| LCL    | 1     | Local segment pointer |
| ARG    | 2     | Argument segment pointer |
| THIS   | 3     | This segment pointer |
| THAT   | 4     | That segment pointer |

## Architecture

The assembler follows SOLID principles with a clean, modular architecture:

```
├── Program.cs                    # Entry point and CLI handling
├── Interfaces/                   # Interface definitions
│   ├── ISymbolTable.cs          # Symbol table operations
│   ├── IInstructionTranslator.cs # Instruction translation
│   └── IAssemblyParser.cs       # File parsing
└── Implementations/             # Concrete implementations
    ├── SymbolTable.cs           # Symbol management
    ├── InstructionTranslator.cs # Assembly to binary translation
    ├── AssemblyParser.cs        # Two-pass parsing logic
    └── HackAssembler.cs         # Main coordinator
```

### Key Components

- **HackAssembler**: Main coordinator that orchestrates the assembly process
- **AssemblyParser**: Handles two-pass parsing (labels first, then code generation)
- **SymbolTable**: Manages predefined symbols, labels, and variables
- **InstructionTranslator**: Converts A-instructions and C-instructions to binary

## Example Program

**Input** (`add.asm`):
```assembly
// Adds 1 + 2 and stores result in R0
@1
D=A
@2
D=D+A
@R0
M=D

// Infinite loop
(END)
@END
0;JMP
```

**Output** (`add_out.asm`):
```
0000000000000001
1110110000010000
0000000000000010
1110000010010000
0000000000000000
1110001100001000
0000000000000110
1110101010000111
```

## Error Handling

The assembler provides clear error messages for common issues:

- Missing input file arguments
- File not found errors
- Invalid instruction formats
- Unknown symbols or labels
- Invalid computation, destination, or jump codes


This project is part of a nand2tetris course implementation.

---
