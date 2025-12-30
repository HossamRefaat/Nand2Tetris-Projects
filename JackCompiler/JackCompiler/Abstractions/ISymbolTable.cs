using JackCompiler.Enums;

namespace JackCompiler.Abstractions
{
    public interface ISymbolTable
    {
        /// <summary>
        /// Starts a new subroutine scope.
        /// Clears previous arguments and local variables.
        /// </summary>
        void StartSubroutine();

        /// <summary>
        /// Defines a new symbol with the specified name, type, and kind and assign it a running index,
        /// in the current context.
        /// </summary>
        /// <param name="name">The name of the symbol to define. Cannot be null or empty.</param>
        /// <param name="type">The data type of the symbol to define. Cannot be null or empty.</param>
        /// <param name="kind">The kind of symbol to define, such as variable, function, or parameter.</param>
        void Define(string name, string type, SymbolKind kind);

        /// <summary>
        /// Returns the number of variables of the specified symbol kind.
        /// </summary>
        /// <param name="kind">The kind of symbol for which to count variables. Must be a valid value of the SymbolKind enumeration.</param>
        /// <returns>The number of variables that match the specified symbol kind. Returns 0 if no variables of the given kind
        /// are present.</returns>
        int VarCount(SymbolKind kind);

        /// <summary>
        /// Determines the kind of symbol associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the symbol to evaluate. Cannot be null or empty.</param>
        /// <returns>A value of the SymbolKind enumeration that indicates the kind of the symbol. Returns SymbolKind.None if the
        /// name does not correspond to a known symbol.</returns>
        SymbolKind KindOf(string name);

        /// <summary>
        /// Returns the type name associated with the specified identifier.
        /// </summary>
        /// <param name="name">The identifier for which to retrieve the type name. Cannot be null or empty.</param>
        /// <returns>A string representing the type name associated with the specified identifier. Returns null if no type is
        /// found for the given name.</returns>
        string? TypeOf(string name);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified name within the collection.
        /// </summary>
        /// <param name="name">The name to locate in the collection. Cannot be null.</param>
        /// <returns>The zero-based index of the first occurrence of the specified name if found; otherwise, -1.</returns>
        int IndexOf(string name);
    }
}
