using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;

namespace Bacon.Compiler.Parsing;

public sealed partial class Parser
{
    private static readonly int AssignmentBoundaryPrecedence = ComputeAssignmentBoundary();

    private static int ComputeAssignmentBoundary()
    {
        if (!BinaryOperators.TryGetValue(TokenType.Is, out var info))
            throw new InvalidOperationException(
                "BinaryOperators must contain TokenType.Is for AssignmentBoundaryPrecedence");

        return info.Precedence + 1;
    }

    private Statement ParseStatement()
    {
        return Current.Type switch
        {
            TokenType.Fast or TokenType.Open => ParseVariableDeclaration(),
            TokenType.If => ParseIfStatement(),
            TokenType.ForEach => ParseForEachStatement(),
            TokenType.While => ParseWhileStatement(),
            TokenType.Return => ParseReturnStatement(),
            TokenType.Throw => ParseThrowStatement(),
            _ => ParseExpressionOrAssignment()
        };
    }

    private ReturnStatement ParseReturnStatement()
    {
        var token = Expect(TokenType.Return, "leverer");

        // If next token is } or EndOfFile, is it "leverer" alone
        if (Check(TokenType.RightBrace) || IsAtEnd())
        {
            return new ReturnStatement(null, token.Line);
        }

        var value = ParseExpression();
        return new ReturnStatement(value, token.Line);
    }

    private ThrowStatement ParseThrowStatement()
    {
        var token = Expect(TokenType.Throw, "kast");
        var value = ParseExpression();
        return new ThrowStatement(value, token.Line);
    }

    // hvis x { ... } ellers hvis y { ... } ellers { ... }
    private IfStatement ParseIfStatement()
    {
        var ifToken = Expect(TokenType.If, "hvis");
        var condition = ParseExpression();
        var thenBranch = ParseBlock();

        IReadOnlyList<Statement>? elseBranch = null;
        if (!Match(TokenType.Else))
            return new IfStatement(condition, thenBranch, elseBranch, ifToken.Line);

        // "ellers hvis" - recursive
        elseBranch = Check(TokenType.If) ? [ParseIfStatement()] : ParseBlock();

        return new IfStatement(condition, thenBranch, elseBranch, ifToken.Line);
    }

    // for hver x i liste { ... }
    private ForEachStatement ParseForEachStatement()
    {
        var token = Expect(TokenType.ForEach, "for hver");
        var variable = Expect(TokenType.Identifier, "loop variable");

        // Check that the next token is Identifier with name "i"
        var inToken = Expect(TokenType.Identifier, "'i'");
        if (inToken.OriginalValue != "i")
            throw new ParseException($"Expected 'i', got '{inToken.OriginalValue}'", inToken.Line, inToken.Column);

        var iterable = ParseExpression();
        var body = ParseBlock();

        return new ForEachStatement(variable.OriginalValue!, iterable, body, token.Line);
    }

    // så lenge x { ... }
    private WhileStatement ParseWhileStatement()
    {
        var token = Expect(TokenType.While, "så lenge");
        var condition = ParseExpression();
        var body = ParseBlock();

        return new WhileStatement(condition, body, token.Line);
    }

    // An expression, optionally followed by "er <value>" for assignment
    private Statement ParseExpressionOrAssignment()
    {
        var line = Current.Line;
        var expr = ParseExpression(AssignmentBoundaryPrecedence);

        if (!Match(TokenType.Is))
            return new ExpressionStatement(expr, line);

        // Validate that the target is something that can be assigned to
        if (expr is not (VariableExpression or FieldAccessExpression))
            throw new ParseException(
                $"Cannot assign to expression of type {expr.GetType().Name}",
                line, Current.Column);

        var value = ParseExpression();
        return new AssignmentStatement(expr, value, line);
    }

    // fast x er 5  /  åpen y er 10
    private VariableDeclarationStatement ParseVariableDeclaration()
    {
        var firstToken = Current;
        var isImmutable = firstToken.Type == TokenType.Fast;
        Advance();  // forbi fast eller åpen

        var nameToken = Expect(TokenType.Identifier, "variable name");
        Expect(TokenType.Is, "er");
        var value = ParseExpression();

        return new VariableDeclarationStatement(
            nameToken.OriginalValue!,
            isImmutable,
            value,
            firstToken.Line);
    }

    // { stmt1 stmt2 ... }
    private IReadOnlyList<Statement> ParseBlock()
    {
        Expect(TokenType.LeftBrace, "{");

        var statements = new List<Statement>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(ParseStatement());
        }

        Expect(TokenType.RightBrace, "}");
        return statements;
    }
}