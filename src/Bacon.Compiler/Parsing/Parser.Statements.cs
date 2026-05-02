using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;

namespace Bacon.Compiler.Parsing;

public sealed partial class Parser
{
    private const int AssignmentBoundaryPrecedence = 31;

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

        // Hvis neste token er } eller EndOfFile, er det "leverer" alene
        if (Check(TokenType.RightBrace) || IsAtEnd())
        {
            return new ReturnStatement(null, token.Line);
        }

        var value = ParseExpression();
        return new ReturnStatement(value, token.Line);
    }

    // kast "feil"
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

        // "ellers hvis" — recursive
        elseBranch = Check(TokenType.If) ? [ParseIfStatement()] : ParseBlock();

        return new IfStatement(condition, thenBranch, elseBranch, ifToken.Line);
    }

    // for hver x i liste { ... }
    private ForEachStatement ParseForEachStatement()
    {
        var token = Expect(TokenType.ForEach, "for hver");
        var variable = Expect(TokenType.Identifier, "loop variable");
        Expect(TokenType.In, "i");
        var iterable = ParseExpression();
        var body = ParseBlock();

        return new ForEachStatement(
            variable.OriginalValue!,
            iterable,
            body,
            token.Line);
    }

    // så lenge x { ... }
    private WhileStatement ParseWhileStatement()
    {
        var token = Expect(TokenType.While, "så lenge");
        var condition = ParseExpression();
        var body = ParseBlock();

        return new WhileStatement(condition, body, token.Line);
    }

    // Et uttrykk, evt. fulgt av "er <verdi>" for assignment
    private Statement ParseExpressionOrAssignment()
    {
        var line = Current.Line;
        var expr = ParseExpression(AssignmentBoundaryPrecedence);

        if (!Match(TokenType.Is))
            return new ExpressionStatement(expr, line);

        // x er 5
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859",
        Justification = "API consistency with AST nodes")]
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