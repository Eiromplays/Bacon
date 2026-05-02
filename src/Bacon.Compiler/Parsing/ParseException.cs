namespace Bacon.Compiler.Parsing;

public sealed class ParseException(string message, int line, int column)
    : Exception(message)
{
    public int Line => line;
    public int Column => column;

    public override string ToString() =>
        $"Parse error at line {Line}, column {Column}: {Message}";
}