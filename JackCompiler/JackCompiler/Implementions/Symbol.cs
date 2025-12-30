using JackCompiler.Enums;

public sealed class Symbol
{
    public string Type { get; }
    public SymbolKind Kind { get; }
    public int Index { get; }

    public Symbol(string type, SymbolKind kind, int index)
    {
        Type = type;
        Kind = kind;
        Index = index;
    }
}
