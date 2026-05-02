namespace Bacon.Compiler.Lexing;

public sealed class LexerException(string message, int line, int column, string? fileName = null)
    : Exception(message)
{
    public int Line => line;
    public int Column => column;
    public string? FileName => fileName;

    public override string ToString() =>
        FileName is null
            ? $"Lexer error at line {Line}, column {Column}: {Message}"
            : $"{FileName}({Line},{Column}): Lexer error: {Message}";
}