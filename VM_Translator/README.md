# VM Translator (Nand2Tetris Project 7–8)

## Overview

This project is part of the [Nand2Tetris](https://www.nand2tetris.org/) course.  
The **VM Translator** is a key component in this journey, converting Virtual Machine (VM) commands into Hack Assembly code that can be executed by the Hack CPU.

The translator is implemented in **C# (.NET 8)**.

---

## Understanding the Virtual Machine

The **Virtual Machine (VM)** acts as a bridge between high-level programming languages (like Jack) and low-level assembly language. It abstracts the underlying hardware, offering a platform-independent layer that defines operations such as arithmetic, memory access, program flow, and function calls.

The VM model used in Nand2Tetris is a **stack-based machine**.

### What Is a Stack Machine?

A **stack machine** is a type of abstract computer that uses a stack data structure to evaluate expressions and manage data.  
All operations are performed using a stack — values are *pushed* onto the stack and *popped* off when needed for computation.  

For example:
- To compute `2 + 3`, the VM pushes `2`, pushes `3`, and then executes the `add` command.  
- The `add` command pops both values, adds them, and pushes the result (`5`) back onto the stack.

This design simplifies code generation and function call management.

> For more details on how the stack machine works in Nand2Tetris, see this excellent blog post:  
> [Nand2Tetris, Part 2.1 — Stack Machine](https://fkfd.me/projects/nand2tetris_2.1/)

---

## Features

- Translates all VM commands into Hack Assembly.
- Supports both **single-file** and **multi-file (directory)** input.
- Implements all VM command types:
  - Arithmetic and logical operations
  - Memory access (push/pop)
  - Program flow (label, goto, if-goto)
  - Function calls and returns
- Includes **bootstrap code** for initializing the stack and calling `Sys.init`.

---

## How It Works

1. **Parser**  
   Reads each `.vm` file line by line, identifying command types and arguments.

2. **CodeWriter**  
   Converts each VM command into corresponding Hack Assembly instructions.

3. **Program.cs**  
   Handles input/output logic, determines if the input path is a file or a directory, and coordinates the translation process.

---

## Usage

### Running from PowerShell or Command Line

#### Single `.vm` file:
```bash
dotnet run "path\to\YourFile.vm"
```

#### Directory containing multiple `.vm` files:
```bash
dotnet run "path\to\YourDirectory"
```

The translator will output a single `.asm` file in the same directory.

---

## Example

**Input (`SimpleAdd.vm`):**
```
push constant 2
push constant 3
add
```

**Output (`SimpleAdd.asm`):**
```
// push constant 2
@2
D=A
@SP
A=M
M=D
@SP
M=M+1

// push constant 3
@3
D=A
@SP
A=M
M=D
@SP
M=M+1

// add
@SP
AM=M-1
D=M
A=A-1
M=M+D
```

---
