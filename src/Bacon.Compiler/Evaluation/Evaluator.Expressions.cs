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
            VariableExpression v => _current.Get(v.Name),
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
            BinaryOperator.Plus => Add(left, right),
            BinaryOperator.Minus => Subtract(left, right),
            BinaryOperator.Star => Multiply(left, right),
            BinaryOperator.Slash => Divide(left, right),
            BinaryOperator.Percent => Modulo(left, right),

            BinaryOperator.Equal => new BaconBoolean(AreEqual(left, right)),
            BinaryOperator.NotEqual => new BaconBoolean(!AreEqual(left, right)),

            BinaryOperator.Greater => Compare(left, right, (a, b) => a > b, (a, b) => a > b),
            BinaryOperator.Less => Compare(left, right, (a, b) => a < b, (a, b) => a < b),
            BinaryOperator.GreaterOrEqual => Compare(left, right, (a, b) => a >= b, (a, b) => a >= b),
            BinaryOperator.LessOrEqual => Compare(left, right, (a, b) => a <= b, (a, b) => a <= b),

            BinaryOperator.And => LogicalAnd(left, right),
            BinaryOperator.Or => LogicalOr(left, right),

            _ => throw new RuntimeException($"Unknown binary operator: {expr.Op}")
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
                _ => throw new RuntimeException($"Cannot negate {TypeName(operand)}")
            },
            UnaryOperator.Not => operand switch
            {
                BaconBoolean b => new BaconBoolean(!b.Value),
                _ => throw new RuntimeException($"'ikke' requires boolean, got {TypeName(operand)}")
            },
            _ => throw new RuntimeException($"Unknown unary operator: {expr.Op}")
        };
    }

    private BaconValue EvaluateFieldAccess(FieldAccessExpression expr)
    {
        var target = EvaluateExpression(expr.Target);

        return target switch
        {
            BaconBesetningInstance instance => GetField(instance, expr.FieldName),
            BaconPathParameters pathParams => GetPathParam(pathParams, expr.FieldName),
            _ => throw new RuntimeException($"Cannot access field on {TypeName(target)}")
        };
    }

    private static BaconValue GetField(BaconBesetningInstance instance, string fieldName)
    {
        return !instance.Fields.TryGetValue(fieldName, out var value) ? throw new RuntimeException($"'{instance.Type.Declaration.Name}' has no field '{fieldName}'") : value;
    }

    private static BaconValue GetPathParam(BaconPathParameters pathParams, string name)
    {
        return !pathParams.Parameters.TryGetValue(name, out var value) ? throw new RuntimeException($"No path parameter named '{name}'") : value;
    }


    private static BaconValue Add(BaconValue left, BaconValue right) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value + r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value + r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value + r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value + r.Value),
        (BaconString l, BaconString r) => new BaconString(l.Value + r.Value),
        _ => throw new RuntimeException($"Cannot add {TypeName(left)} and {TypeName(right)}")
    };

    private static BaconValue Subtract(BaconValue left, BaconValue right) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value - r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value - r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value - r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value - r.Value),
        _ => throw new RuntimeException($"Cannot subtract {TypeName(right)} from {TypeName(left)}")
    };

    private static BaconValue Multiply(BaconValue left, BaconValue right) => (left, right) switch
    {
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value * r.Value),
        (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value * r.Value),
        (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value * r.Value),
        (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value * r.Value),
        _ => throw new RuntimeException($"Cannot multiply {TypeName(left)} and {TypeName(right)}")
    };

    private static BaconValue Divide(BaconValue left, BaconValue right)
    {
        // Guard against divide by zero
        if (right is BaconInteger { Value: 0 } or BaconDecimal { Value: 0.0 })
            throw new RuntimeException("Division by zero");

        return (left, right) switch
        {
            (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value / r.Value),
            (BaconDecimal l, BaconDecimal r) => new BaconDecimal(l.Value / r.Value),
            (BaconInteger l, BaconDecimal r) => new BaconDecimal(l.Value / r.Value),
            (BaconDecimal l, BaconInteger r) => new BaconDecimal(l.Value / r.Value),
            _ => throw new RuntimeException($"Cannot divide {TypeName(left)} by {TypeName(right)}")
        };
    }

    private static BaconInteger Modulo(BaconValue left, BaconValue right) => (left, right) switch
    {
        (BaconInteger l, BaconInteger { Value: 0 }) => throw new RuntimeException("Modulo by zero"),
        (BaconInteger l, BaconInteger r) => new BaconInteger(l.Value % r.Value),
        _ => throw new RuntimeException($"Modulo only supported for integers")
    };

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
        _ => false  // different types never match
    };

    private static BaconBoolean Compare(
        BaconValue left,
        BaconValue right,
        Func<long, long, bool> intComparer,
        Func<double, double, bool> doubleComparer)
    {
        return (left, right) switch
        {
            (BaconInteger l, BaconInteger r) => new BaconBoolean(intComparer(l.Value, r.Value)),
            (BaconDecimal l, BaconDecimal r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            (BaconInteger l, BaconDecimal r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            (BaconDecimal l, BaconInteger r) => new BaconBoolean(doubleComparer(l.Value, r.Value)),
            _ => throw new RuntimeException($"Cannot compare {TypeName(left)} and {TypeName(right)}")
        };
    }

    private static BaconBoolean LogicalAnd(BaconValue left, BaconValue right)
    {
        if (left is not BaconBoolean l || right is not BaconBoolean r)
            throw new RuntimeException($"'og' requires booleans, got {TypeName(left)} and {TypeName(right)}");
        return new BaconBoolean(l.Value && r.Value);
    }

    private static BaconBoolean LogicalOr(BaconValue left, BaconValue right)
    {
        if (left is not BaconBoolean l || right is not BaconBoolean r)
            throw new RuntimeException($"'eller' requires booleans, got {TypeName(left)} and {TypeName(right)}");
        return new BaconBoolean(l.Value || r.Value);
    }
}