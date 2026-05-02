using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;

namespace Bacon.Compiler.Parsing;

public sealed partial class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _current;

    private Parser(IReadOnlyList<Token> tokens) => _tokens = tokens;

    public static Program Parse(IReadOnlyList<Token> tokens) =>
        new Parser(tokens).ParseProgram();

    internal static Expression ParseExpressionOnly(IReadOnlyList<Token> tokens) =>
        new Parser(tokens).ParseExpression();

    internal static Statement ParseStatementOnly(IReadOnlyList<Token> tokens) =>
        new Parser(tokens).ParseStatement();

    private static readonly Dictionary<TokenType, (BinaryOperator Op, int Precedence)> BinaryOperators = new()
    {
        // Logikk (lavest)
        [TokenType.Or]              = (BinaryOperator.Or, 10),
        [TokenType.And]             = (BinaryOperator.And, 20),

        // Likhet
        [TokenType.Is]              = (BinaryOperator.Equal, 30),
        [TokenType.NotEqual]        = (BinaryOperator.NotEqual, 30),

        // Sammenligning
        [TokenType.GreaterThan]     = (BinaryOperator.Greater, 40),
        [TokenType.LessThan]        = (BinaryOperator.Less, 40),
        [TokenType.GreaterOrEqual]  = (BinaryOperator.GreaterOrEqual, 40),
        [TokenType.LessOrEqual]     = (BinaryOperator.LessOrEqual, 40),

        // Addisjon
        [TokenType.Plus]            = (BinaryOperator.Plus, 50),
        [TokenType.Minus]           = (BinaryOperator.Minus, 50),

        // Multiplikasjon (høyest)
        [TokenType.Star]            = (BinaryOperator.Star, 60),
        [TokenType.Slash]           = (BinaryOperator.Slash, 60),
        [TokenType.Percent]         = (BinaryOperator.Percent, 60),
    };

    private Program ParseProgram()
    {
        var declarations = new List<Declaration>();

        while (!IsAtEnd())
        {
            declarations.Add(ParseDeclaration());
        }

        return new Program(declarations, 1);
    }

    private bool IsAtEnd() => Current.Type == TokenType.EndOfFile;

    private Token Current => _tokens[_current];

    private Token Peek(int offset = 1) =>
        _current + offset < _tokens.Count
            ? _tokens[_current + offset]
            : _tokens[^1];   // siste token (EndOfFile)

    private Token Advance()
    {
        var token = Current;
        if (!IsAtEnd()) _current++;
        return token;
    }

    private bool Check(TokenType type) => Current.Type == type;

    private bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }

    private Token Expect(TokenType type, string expectedDescription)
    {
        if (Check(type)) return Advance();
        throw new ParseException(
            $"Expected {expectedDescription}, got '{Current.OriginalValue}'",
            Current.Line, Current.Column);
    }
}