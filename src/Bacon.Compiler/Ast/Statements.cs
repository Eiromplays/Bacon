namespace Bacon.Compiler.Ast;

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