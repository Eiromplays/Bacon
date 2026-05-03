namespace Bacon.Compiler.Ast;

public abstract record AstNode(int Line);

public sealed record Program(IReadOnlyList<Declaration> Declarations, int Line) : AstNode(Line);

// Declarations
public abstract record Declaration(int Line) : AstNode(Line);

// Statements
public abstract record Statement(int Line) : AstNode(Line);

// Expressions
public abstract record Expression(int Line) : AstNode(Line);