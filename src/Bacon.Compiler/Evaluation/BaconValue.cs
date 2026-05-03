using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public abstract record BaconValue;

public sealed record BaconInteger(long Value) : BaconValue;
public sealed record BaconDecimal(double Value) : BaconValue;
public sealed record BaconString(string Value) : BaconValue;
public sealed record BaconBoolean(bool Value) : BaconValue;
public sealed record BaconNothing : BaconValue
{
    public static readonly BaconNothing Instance = new();
}
public sealed record BaconList(List<BaconValue> Elements) : BaconValue;
public sealed record BaconBesetningInstance(
    BaconBesetningType Type,
    Dictionary<string, BaconValue> Fields) : BaconValue
{
    public string TypeName => Type.Declaration.Name;
}
public sealed record BaconBesetningType(BesetningDeclaration Declaration) : BaconValue;

public sealed record BaconProcess(
    ProcessDeclaration Declaration,
    Environment Closure) : BaconValue;

public sealed record BaconBuiltinFunction(
    string Name,
    Func<IReadOnlyList<BaconValue>, BaconValue> Implementation) : BaconValue;