using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    private BaconValue EvaluateCall(CallExpression call)
    {
        var callee = EvaluateExpression(call.Callee);
        var args = call.Arguments.Select(EvaluateExpression).ToList();

        return callee switch
        {
            BaconProcess process => CallProcess(process, args),
            BaconBuiltinFunction builtin => builtin.Implementation(args),
            BaconBesetningType type => InstantiateBesetning(type, args),
            _ => throw new RuntimeException($"Cannot call value of type {TypeName(callee)}")
        };
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859",
        Justification = "We do not need to modify the list just read it")]
    private static BaconBesetningInstance InstantiateBesetning(
        BaconBesetningType type,
        IReadOnlyList<BaconValue> args)
    {
        var decl = type.Declaration;

        if (args.Count != decl.Fields.Count)
        {
            throw new RuntimeException(
                $"'{decl.Name}' expected {decl.Fields.Count} arguments, got {args.Count}");
        }

        var fields = new Dictionary<string, BaconValue>();
        for (var i = 0; i < decl.Fields.Count; i++)
        {
            fields[decl.Fields[i].Name] = args[i];
        }

        return new BaconBesetningInstance(type, fields);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859",
        Justification = "We do not need to modify the list just read it")]
    private BaconValue CallProcess(BaconProcess process, IReadOnlyList<BaconValue> args)
    {
        var decl = process.Declaration;

        if (args.Count != decl.Parameters.Count)
        {
            throw new RuntimeException(
                $"'{decl.Name}' expected {decl.Parameters.Count} arguments, got {args.Count}");
        }

        var callEnv = new Environment(parent: process.Closure);

        for (var i = 0; i < decl.Parameters.Count; i++)
        {
            callEnv.Define(decl.Parameters[i].Name, args[i]);
        }

        var savedEnv = _current;
        _current = callEnv;

        try
        {
            EvaluateBlock(decl.Body);
            return BaconNothing.Instance;
        }
        catch (ReturnException ret)
        {
            return ret.Value;
        }
        finally
        {
            _current = savedEnv;
        }
    }

    private void DefineStdlib()
    {
        Define("skriv", args =>
        {
            Console.WriteLine(string.Join(" ", args.Select(FormatValue)));
            return BaconNothing.Instance;
        });

        Define("lengde", args =>
        {
            if (args.Count != 1)
                throw new RuntimeException("lengde tar 1 argument");

            return args[0] switch
            {
                BaconString s => new BaconInteger(s.Value.Length),
                BaconList l => new BaconInteger(l.Elements.Count),
                _ => throw new RuntimeException($"lengde støttes ikke for {TypeName(args[0])}")
            };
        });

        Define("til_tekst",
            args => args.Count != 1
                ? throw new RuntimeException("til_tekst tar 1 argument")
                : new BaconString(FormatValue(args[0])));
    }

    private void Define(string name, Func<IReadOnlyList<BaconValue>, BaconValue> impl)
    {
        _global.Define(name, new BaconBuiltinFunction(name, impl), isImmutable: true);
    }
}