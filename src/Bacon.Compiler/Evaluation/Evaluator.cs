using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    private readonly Environment _global = new();
    private Environment _current;

    public Evaluator()
    {
        _current = _global;
        DefineStdlib();
    }

    public static BaconValue Evaluate(Program program)
    {
        var evaluator = new Evaluator();
        return evaluator.EvaluateProgram(program);
    }

    public void EvaluateStatements(IReadOnlyList<Statement> statements)
    {
        EvaluateBlock(statements);
    }

    private BaconValue EvaluateProgram(Program program)
    {
        foreach (var decl in program.Declarations)
        {
            EvaluateDeclaration(decl);
        }

        // Hvis det finnes en "hovedprogram"-prosess, kjør den
        if (!_current.IsDefined("hovedprogram"))
            return BaconNothing.Instance;

        var hovedprogram = _current.Get("hovedprogram");
        if (hovedprogram is BaconProcess proc)
        {
            return CallProcess(proc, []);
        }

        return BaconNothing.Instance;
    }

    private void EvaluateDeclaration(Declaration decl)
    {
        switch (decl)
        {
            case ProcessDeclaration proc:
                // Definer prosessen som en variabel i global scope
                _global.Define(proc.Name, new BaconProcess(proc, _global), isImmutable: true);
                break;

            case BesetningDeclaration:
            case ImportDeclaration:
            case RouteDeclaration:
                // TODO: håndter senere
                break;

            default:
                throw new RuntimeException($"Unknown declaration: {decl.GetType().Name}");
        }
    }
}