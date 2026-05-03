using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    public void PrepareGlobalScope(Program program)
    {
        foreach (var decl in program.Declarations)
        {
            EvaluateDeclaration(decl);
        }
    }

    public BaconValue EvaluateRoute(
        RouteDeclaration route,
        IReadOnlyDictionary<string, string> pathParameters)
    {
        var routeEnv = new Environment(parent: _global);

        var paramFields = new Dictionary<string, BaconValue>();
        foreach (var (key, value) in pathParameters)
        {
            paramFields[key] = new BaconString(value);
        }

        routeEnv.Define("parameter", new BaconPathParameters(paramFields));

        var savedEnv = _current;
        _current = routeEnv;

        try
        {
            EvaluateBlock(route.Statements);
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
}