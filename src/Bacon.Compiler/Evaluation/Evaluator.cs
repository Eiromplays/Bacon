using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    private const string MainProgramName = "hovedprogram";

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

        if (!_current.IsDefined(MainProgramName))
            return BaconNothing.Instance;

        var mainProgram = _current.Get(MainProgramName);
        if (mainProgram is BaconProcess proc)
        {
            return CallProcess(proc, [], proc.Declaration.Line);
        }

        return BaconNothing.Instance;
    }

    private void EvaluateDeclaration(Declaration decl)
    {
        switch (decl)
        {
            case ProcessDeclaration proc:
                _global.Define(proc.Name, new BaconProcess(proc, _global), isImmutable: true);
                break;

            case BesetningDeclaration besetning:
                _global.Define(besetning.Name, new BaconBesetningType(besetning), isImmutable: true);
                break;
            case RouteDeclaration:
                // Routes are registered by Bacon.Web, not evaluated as global state
                break;
            case ImportDeclaration:
                // Import system not yet implemented, all stdlib is global
                break;

            default:
                throw new RuntimeException($"Unknown declaration: {decl.GetType().Name}");
        }
    }
}