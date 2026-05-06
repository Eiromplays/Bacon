using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    public BaconValue EvaluateStatement(Statement stmt)
    {
        return stmt switch
        {
            VariableDeclarationStatement vd => EvaluateVariableDeclaration(vd),
            AssignmentStatement assign => EvaluateAssignment(assign),
            ExpressionStatement exprStmt => EvaluateExpression(exprStmt.Expression),
            IfStatement ifStmt => EvaluateIf(ifStmt),
            ForEachStatement forEach => EvaluateForEach(forEach),
            WhileStatement whileStmt => EvaluateWhile(whileStmt),
            ReturnStatement ret => throw new ReturnException(
                ret.Value is not null
                    ? EvaluateExpression(ret.Value)
                    : BaconNothing.Instance),
            ThrowStatement thr => EvaluateThrow(thr),
            _ => throw new RuntimeException($"Unknown statement: {stmt.GetType().Name}", stmt.Line)
        };
    }

    private BaconNothing EvaluateIf(IfStatement stmt)
    {
        var condition = EvaluateExpression(stmt.Condition);

        if (condition is not BaconBoolean b)
            throw new RuntimeException($"If condition must be boolean, got {TypeName(condition)}", stmt.Line);

        if (b.Value)
        {
            EvaluateBlock(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch is not null)
        {
            EvaluateBlock(stmt.ElseBranch);
        }

        return BaconNothing.Instance;
    }

    private BaconNothing EvaluateWhile(WhileStatement stmt)
    {
        while (true)
        {
            var condition = EvaluateExpression(stmt.Condition);

            if (condition is not BaconBoolean b)
                throw new RuntimeException($"While condition must be boolean, got {TypeName(condition)}", stmt.Line);

            if (!b.Value) break;

            EvaluateBlock(stmt.Body);
        }

        return BaconNothing.Instance;
    }

    private BaconNothing EvaluateForEach(ForEachStatement stmt)
    {
        var iterable = EvaluateExpression(stmt.Iterable);

        if (iterable is not BaconList list)
            throw new RuntimeException($"ForEach requires a list, got {TypeName(iterable)}", stmt.Line);

        foreach (var element in list.Elements)
        {
            // New scope per iteration so the loop variable doesn't leak, and can be redefined each time
            var savedEnv = _current;
            _current = new Environment(parent: savedEnv);

            try
            {
                _current.Define(stmt.Variable, element);
                EvaluateBlock(stmt.Body);
            }
            finally
            {
                _current = savedEnv;
            }
        }

        return BaconNothing.Instance;
    }

    private BaconNothing EvaluateVariableDeclaration(VariableDeclarationStatement stmt)
    {
        var value = EvaluateExpression(stmt.Value);

        try
        {
            _current.Define(stmt.Name, value, isImmutable: stmt.IsImmutable);
        }
        catch (RuntimeException ex) when (!ex.Line.HasValue)
        {
            throw new RuntimeException(ex.Message, stmt.Line);
        }

        return BaconNothing.Instance;
    }

    private BaconNothing EvaluateAssignment(AssignmentStatement stmt)
    {
        var value = EvaluateExpression(stmt.Value);

        switch (stmt.Target)
        {
            case VariableExpression varExpr:
                try
                {
                    _current.Assign(varExpr.Name, value);
                }
                catch (RuntimeException ex) when (!ex.Line.HasValue)
                {
                    throw new RuntimeException(ex.Message, stmt.Line);
                }
                break;

            case FieldAccessExpression fieldAccess:
                AssignField(fieldAccess, value, stmt.Line);
                break;

            default:
                throw new RuntimeException("Assignment target must be a variable or field access", stmt.Line);
        }

        return BaconNothing.Instance;
    }

    private void AssignField(FieldAccessExpression fieldAccess, BaconValue value, int line)
    {
        var target = EvaluateExpression(fieldAccess.Target);

        if (target is not BaconBesetningInstance instance)
            throw new RuntimeException($"Cannot assign field on {TypeName(target)}", line);

        var fieldDecl = instance.Type.Declaration.Fields
            .FirstOrDefault(f => f.Name == fieldAccess.FieldName);

        if (fieldDecl == null)
            throw new RuntimeException(
                $"'{instance.TypeName}' has no field '{fieldAccess.FieldName}'", line);

        if (fieldDecl.IsImmutable)
            throw new RuntimeException(
                $"Cannot reassign immutable field '{fieldAccess.FieldName}' on '{instance.TypeName}'", line);

        instance.Fields[fieldAccess.FieldName] = value;
    }

    private BaconValue EvaluateThrow(ThrowStatement stmt)
    {
        var value = EvaluateExpression(stmt.Value);

        var message = value switch
        {
            BaconString s => s.Value,
            _ => $"Thrown value: {value}"
        };

        throw new RuntimeException(message, stmt.Line);
    }

    private void EvaluateBlock(IReadOnlyList<Statement> statements)
    {
        foreach (var stmt in statements)
        {
            EvaluateStatement(stmt);
        }
    }
}