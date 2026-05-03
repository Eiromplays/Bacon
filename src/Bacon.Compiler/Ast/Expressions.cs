namespace Bacon.Compiler.Ast;

// Literals
public sealed record IntegerLiteralExpression(long Value, int Line) : Expression(Line);
public sealed record DecimalLiteralExpression(double Value, int Line) : Expression(Line);
public sealed record StringLiteralExpression(string Value, int Line) : Expression(Line);
public sealed record BooleanLiteralExpression(bool Value, int Line) : Expression(Line);
public sealed record NothingLiteralExpression(int Line) : Expression(Line);

// Variabel-referanse
public sealed record VariableExpression(string Name, int Line) : Expression(Line);

// Binær (a + b, a større enn b, etc.)
public sealed record BinaryExpression(Expression Left, BinaryOperator Op, Expression Right, int Line) : Expression(Line);

// Unær (-a, ikke b)
public sealed record UnaryExpression(UnaryOperator Op, Expression Operand, int Line) : Expression(Line);

// Funksjons-/metode-kall: foo(a, b) eller obj.foo(a, b)
public sealed record CallExpression(Expression Callee, IReadOnlyList<Expression> Arguments, int Line) : Expression(Line);

// Felt-tilgang: bil.modell
public sealed record FieldAccessExpression(Expression Target, string FieldName, int Line) : Expression(Line);

// Liste-literal: [1, 2, 3]
public sealed record ListExpression(IReadOnlyList<Expression> Elements, int Line) : Expression(Line);

// Operatorer
public enum BinaryOperator
{
    Plus, Minus, Star, Slash, Percent,         // aritmetikk
    Equal, NotEqual,                            // likhet
    Greater, Less, GreaterOrEqual, LessOrEqual, // sammenligning
    And, Or,                                    // logikk
}

public enum UnaryOperator
{
    Negate,    // -x
    Not,       // ikke x
}