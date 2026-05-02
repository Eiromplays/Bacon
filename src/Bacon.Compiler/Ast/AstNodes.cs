namespace Bacon.Compiler.Ast;

public abstract record AstNode(int Line);

public sealed record Program(IReadOnlyList<Declaration> Declarations, int Line) : AstNode(Line);

// Declarations
public abstract record Declaration(int Line) : AstNode(Line);

// Statements
public abstract record Statement(int Line) : AstNode(Line);

// Expressions
public abstract record Expression(int Line) : AstNode(Line);

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

// fast x er 5  /  åpen y er 10
public sealed record VariableDeclarationStatement(
    string Name,
    bool IsImmutable,           // fast = true, åpen = false
    Expression Value,
    int Line) : Statement(Line);

// x er 5 (re-assignment, kun for åpen)
public sealed record AssignmentStatement(
    Expression Target,
    Expression Value,
    int Line) : Statement(Line);

// Et uttrykk som statement (typisk funksjonskall)
public sealed record ExpressionStatement(Expression Expression, int Line) : Statement(Line);

// hvis x { ... } ellers hvis y { ... } ellers { ... }
public sealed record IfStatement(
    Expression Condition,
    IReadOnlyList<Statement> ThenBranch,
    IReadOnlyList<Statement>? ElseBranch,
    int Line) : Statement(Line);

// for hver x i liste { ... }
public sealed record ForEachStatement(
    string Variable,
    Expression Iterable,
    IReadOnlyList<Statement> Body,
    int Line) : Statement(Line);

// så lenge x { ... }
public sealed record WhileStatement(
    Expression Condition,
    IReadOnlyList<Statement> Body,
    int Line) : Statement(Line);

// leverer x
public sealed record ReturnStatement(Expression? Value, int Line) : Statement(Line);

// kast "feil"
public sealed record ThrowStatement(Expression Value, int Line) : Statement(Line);

// import drift.tjener
// import drift.tjener som t
public sealed record ImportDeclaration(string Path, string? Alias, int Line) : Declaration(Line);

// besetning Bil { fast id : tekst, åpen modell : tekst }
public sealed record BesetningDeclaration(
    string Name,
    IReadOnlyList<FieldDeclaration> Fields,
    int Line) : Declaration(Line);

// Et felt inni en besetning
public sealed record FieldDeclaration(
    string Name,
    string TypeName,            // "tekst", "heltall", etc.
    bool IsImmutable,           // fast vs åpen
    int Line) : AstNode(Line);

// prosess summer(a, b) { ... }
public sealed record ProcessDeclaration(
    string Name,
    IReadOnlyList<ParameterDeclaration> Parameters,
    IReadOnlyList<Statement> Body,
    int Line) : Declaration(Line);

public sealed record ParameterDeclaration(
    string Name,
    string? TypeName,          // type-annotering er valgfri
    int Line) : AstNode(Line);

// rute GET "/bil/{id}" mottar data : NyBil { ... }
public sealed record RouteDeclaration(
    string HttpMethod,                       // "GET", "POST", etc.
    string Path,                              // "/bil/{id}"
    BodyBinding? Body,                        // mottar data : NyBil (valgfri)
    IReadOnlyList<Statement> Statements,
    int Line) : Declaration(Line);

public sealed record BodyBinding(
    string Name,
    string TypeName,
    int Line) : AstNode(Line);