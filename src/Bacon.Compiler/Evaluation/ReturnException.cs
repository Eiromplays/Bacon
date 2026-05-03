namespace Bacon.Compiler.Evaluation;

public sealed class ReturnException(BaconValue value) : Exception
{
    public BaconValue Value { get; } = value;
}