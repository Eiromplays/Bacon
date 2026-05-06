using System.Diagnostics.CodeAnalysis;
using Bacon.Compiler.Ast;

namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    public BaconValue EvaluateExpression(Expression expr)
    {
        return expr switch
        {
            IntegerLiteralExpression i => new BaconInteger(i.Value),
            DecimalLiteralExpression d => new BaconDecimal(d.Value),
            StringLiteralExpression s => new BaconString(s.Value),
            BooleanLiteralExpression b => new BaconBoolean(b.Value),
            NothingLiteralExpression => BaconNothing.Instance,
            VariableExpression v => GetVariable(v),
            ListExpression list => new BaconList(list.Elements.Select(EvaluateExpression).ToList()),
            BinaryExpression bin => EvaluateBinary(bin),
            UnaryExpression un => EvaluateUnary(un),
            CallExpression call => EvaluateCall(call),
            FieldAccessExpression fa => EvaluateFieldAccess(fa),

            _ => throw new NotImplementedException($"Expression: {expr.GetType().Name}")
        };
    }

    private BaconValue EvaluateBinary(BinaryExpression expr)
    {
        var left = EvaluateExpression(expr.Left);
        var right = EvaluateExpression(expr.Right);

        return expr.Op switch
        {
            BinaryOperator.Plus => Add(left, right, expr.Line),
            BinaryOperator.Minus => Subtract(left, right, expr.Line),
            BinaryOperator.Star => Multiply(left, right, expr.Line),
            BinaryOperator.Slash => Divide(left, right, expr.Line),
            BinaryOperator.Percent => Modulo(left, right, expr.Line),

            BinaryOperator.Equal => new BaconBoolean(AreEqual(left, right)),
            BinaryOperator.NotEqual => new BaconBoolean(!AreEqual(left, right)),

            BinaryOperator.Greater => Compare(left, right, (a, b) => a > b, (a, b) => a > b, expr.Line),
            BinaryOperator.Less => Compare(left, right, (a, b) => a < b, (a, b) => a < b, expr.Line),
            BinaryOperator.GreaterOrEqual => Compare(left, right, (a, b) => a >= b, (a, b) => a >= b, expr.Line),
            BinaryOperator.LessOrEqual => Compare(left, right, (a, b) => a <= b, (a, b) => a <= b, expr.Line),

            BinaryOperator.And => LogicalAnd(left, right, expr.Line),
            BinaryOperator.Or => LogicalOr(left, right, expr.Line),

            _ => throw new RuntimeException($"Unknown binary operator: {expr.Op}", expr.Line)
        };
    }

    private BaconValue EvaluateUnary(UnaryExpression expr)
    {
        var operand = EvaluateExpression(expr.Operand);

        return expr.Op switch
        {
            UnaryOperator.Negate => operand switch
            {
                BaconInteger i => new BaconInteger(-i.Value),
                BaconDecimal d => new BaconDecimal(-d.Value),
                _ => throw new RuntimeException($"Cannot negate {TypeName(operand)}", expr.Line)
            },
            UnaryOperator.Not => operand switch
            {
                BaconBoolean b => new BaconBoolean(!b.Value),
                _ => throw new RuntimeException($"'ikke' requires boolean, got {TypeName(operand)}", expr.Line)
            },
            _ => throw new RuntimeException($"Unknown unary operator: {expr.Op}", expr.Line)
        };
    }

    private BaconValue EvaluateFieldAccess(FieldAccessExpression expr)
    {
        var target = EvaluateExpression(expr.Target);

        return target switch
        {
            BaconBesetningInstance instance => GetField(instance, expr.FieldName, expr.Line),
            BaconPathParameters pathParams => GetPathParam(pathParams, expr.FieldName, expr.Line),
            _ => throw new RuntimeException($"Cannot access field on {TypeName(target)}", expr.Line)
        };
    }

    private static BaconValue GetField(BaconBesetningInstance instance, string fieldName, int line)
    {
        return !instance.Fields.TryGetValue(fieldName, out var value)
            ? throw new RuntimeException($"'{instance.Type.Declaration.Name}' has no field '{fieldName}'", line)
            : value;
    }

    private static BaconValue GetPathParam(BaconPathParameters pathParams, string name, int line)
    {
        return !pathParams.Parameters.TryGetValue(name, out var value)
            ? throw new RuntimeException($"No path parameter named '{name}'", line)
            : value;
    }

    private static BaconValue Add(BaconValue left, BaconValue right, int line) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value + r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value + r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value + r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value + r.Value),
        (BaconString l, BaconString r) => new BaconString(l.Value + r.Value),
        _ => throw new RuntimeException($"Cannot add {TypeName(left)} and {TypeName(right)}", line)
    };

    private static BaconValue Subtract(BaconValue left, BaconValue right, int line) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value - r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value - r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value - r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value - r.Value),
        _ => throw new RuntimeException($"Cannot subtract {TypeName(right)} from {TypeName(left)}", line)
    };

    private static BaconValue Multiply(BaconValue left, BaconValue right, int line) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value * r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value * r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value * r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value * r.Value),
        _ => throw new RuntimeException($"Cannot multiply {TypeName(left)} and {TypeName(right)}", line)
    };

    private static BaconValue Divide(BaconValue left, BaconValue right, int line)
    {
        if (right is BaconInteger { Value: 0 } or BaconDecimal { Value: 0.0 })
            throw new RuntimeException("Division by zero", line);

        return (left, right) switch
        {
            (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value / r.Value),
            (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value / r.Value),
            (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value / r.Value),
            (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value / r.Value),
            _ => throw new RuntimeException($"Cannot divide {TypeName(left)} by {TypeName(right)}", line)
        };
    }

    private static BaconInteger Modulo(BaconValue left, BaconValue right, int line) => (left, right) switch
    {
        (BaconInteger _, BaconInteger { Value: 0 }) => throw new RuntimeException("Modulo by zero", line),
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value % r.Value),
        _ => throw new RuntimeException("Modulo only supported for integers", line)
    };

    private static BaconBoolean Compare(
        BaconValue left,
        BaconValue right,
        Func<long, long, bool> intComparer,
        Func<double, double, bool> doubleComparer,
        int line)
    {
        return (left, right) switch
        {
            (BaconInteger l, BaconInteger r) => new BaconBoolean(intComparer(l.Value, r.Value)),
            (BaconDecimal l, BaconDecimal r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            (BaconInteger l, BaconDecimal r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            (BaconDecimal l, BaconInteger r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            _ => throw new RuntimeException($"Cannot compare {TypeName(left)} and {TypeName(right)}", line)
        };
    }

    private static BaconBoolean LogicalAnd(BaconValue left, BaconValue right, int line)
    {
        if (left is not BaconBoolean l || right is not BaconBoolean r)
            throw new RuntimeException($"'og' requires booleans, got {TypeName(left)} and {TypeName(right)}", line);
        return new BaconBoolean(l.Value && r.Value);
    }

    private static BaconBoolean LogicalOr(BaconValue left, BaconValue right, int line)
    {
        if (left is not BaconBoolean l || right is not BaconBoolean r)
            throw new RuntimeException($"'eller' requires booleans, got {TypeName(left)} and {TypeName(right)}", line);
        return new BaconBoolean(l.Value || r.Value);
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Bacon doesn't deal with values large enough for precision loss to matter")]
    private static bool AreEqual(BaconValue left, BaconValue right) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => l.Value == r.Value,
        (BaconDecimal l, BaconDecimal r) => l.Value == r.Value,
        (BaconInteger l, BaconDecimal r) => l.Value == r.Value,
        (BaconDecimal l, BaconInteger r) => l.Value == r.Value,
        (BaconString l, BaconString r) => l.Value == r.Value,
        (BaconBoolean l, BaconBoolean r) => l.Value == r.Value,
        (BaconNothing, BaconNothing) => true,
        (BaconList l, BaconList r) => ListsEqual(l, r),
        (BaconBesetningInstance l, BaconBesetningInstance r) => InstancesEqual(l, r),
        _ => false
    };

    private static bool ListsEqual(BaconList left, BaconList right)
    {
        if (left.Elements.Count != right.Elements.Count) return false;

        return !left.Elements.Where((t, i) => !AreEqual(t, right.Elements[i])).Any();
    }

    private static bool InstancesEqual(BaconBesetningInstance left, BaconBesetningInstance right)
    {
        if (left.Type.Declaration.Name != right.Type.Declaration.Name) return false;
        if (left.Fields.Count != right.Fields.Count) return false;

        foreach (var (key, leftValue) in left.Fields)
        {
            if (!right.Fields.TryGetValue(key, out var rightValue)) return false;
            if (!AreEqual(leftValue, rightValue)) return false;
        }

        return true;
    }

    private BaconValue GetVariable(VariableExpression expr)
    {
        try
        {
            return _current.Get(expr.Name);
        }
        catch (RuntimeException ex) when (!ex.Line.HasValue)
        {
            throw new RuntimeException(ex.Message, expr.Line);
        }
    }
}