using JackCompiler.Abstractions;
using JackCompiler.Enums;

public class SymbolTable : ISymbolTable
{
    // Class-level scope (static, field)
    private readonly Dictionary<string, Symbol> _classScope = new();

    // Subroutine-level scope (arg, var)
    private readonly Dictionary<string, Symbol> _subroutineScope = new();

    // Running index counters
    private readonly Dictionary<SymbolKind, int> _counts = new()
    {
        { SymbolKind.STATIC, 0 },
        { SymbolKind.FIELD, 0 },
        { SymbolKind.ARG, 0 },
        { SymbolKind.VAR, 0 }
    };

    public void StartSubroutine()
    {
        _subroutineScope.Clear();
        _counts[SymbolKind.ARG] = 0;
        _counts[SymbolKind.VAR] = 0;
    }

    public void Define(string name, string type, SymbolKind kind)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        int index = _counts[kind];
        var symbol = new Symbol(type, kind, index);

        if (kind == SymbolKind.STATIC || kind == SymbolKind.FIELD)
            _classScope[name] = symbol;
        else
            _subroutineScope[name] = symbol;

        _counts[kind]++;
    }

    public int VarCount(SymbolKind kind)
    {
        return _counts.TryGetValue(kind, out var count) ? count : 0;
    }

    public SymbolKind KindOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out var subSymbol))
            return subSymbol.Kind;

        if (_classScope.TryGetValue(name, out var classSymbol))
            return classSymbol.Kind;

        return SymbolKind.NONE;
    }

    public string? TypeOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out var subSymbol))
            return subSymbol.Type;

        if (_classScope.TryGetValue(name, out var classSymbol))
            return classSymbol.Type;

        return null;
    }

    public int IndexOf(string name)
    {
        if (_subroutineScope.TryGetValue(name, out var subSymbol))
            return subSymbol.Index;

        if (_classScope.TryGetValue(name, out var classSymbol))
            return classSymbol.Index;

        return -1;
    }
}
