namespace Bacon.Compiler.Evaluation;

public sealed class RuntimeException : Exception
{
    public int? Line { get; }

    public RuntimeException(string message) : base(message) { }

    public RuntimeException(string message, int line) : base(message)
    {
        Line = line;
    }
}