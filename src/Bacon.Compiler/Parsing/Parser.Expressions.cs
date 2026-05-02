using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;

namespace Bacon.Compiler.Parsing;

public sealed partial class Parser
{
    private Expression ParsePrimary()
    {
        var token = Current;

        switch (token.Type)
        {
            case TokenType.IntegerLiteral:
                Advance();
                return new IntegerLiteralExpression((long)token.Value!, token.Line);

            case TokenType.DecimalLiteral:
                Advance();
                return new DecimalLiteralExpression((double)token.Value!, token.Line);

            case TokenType.StringLiteral:
                Advance();
                return new StringLiteralExpression((string)token.Value!, token.Line);

            case TokenType.True:
                Advance();
                return new BooleanLiteralExpression(true, token.Line);

            case TokenType.False:
                Advance();
                return new BooleanLiteralExpression(false, token.Line);

            case TokenType.Nothing:
                Advance();
                return new NothingLiteralExpression(token.Line);

            case TokenType.Identifier:
                Advance();
                return new VariableExpression(token.OriginalValue!, token.Line);

            case TokenType.LeftParen:
                Advance();
                var expr = ParseExpression();
                Expect(TokenType.RightParen, ")");
                return expr;

            case TokenType.LeftBracket:
                return ParseListExpression();

            default:
                throw new ParseException(
                    $"Unexpected token '{token.OriginalValue}' when parsing expression",
                    token.Line, token.Column);
        }
    }

    private Expression ParseExpression(int minPrecedence = 0)
    {
        var left = ParseUnary();

        while (true)
        {
            if (!BinaryOperators.TryGetValue(Current.Type, out var info))
                break;

            if (info.Precedence < minPrecedence)
                break;

            var opToken = Advance();
            var right = ParseExpression(info.Precedence + 1);
            left = new BinaryExpression(left, info.Op, right, opToken.Line);
        }

        return left;
    }

    private Expression ParseUnary()
    {
        if (Match(TokenType.Minus))
        {
            var line = _tokens[_current - 1].Line;
            var operand = ParseUnary();   // recursive: tillater --x (selv om sjeldent)
            return new UnaryExpression(UnaryOperator.Negate, operand, line);
        }

        if (Match(TokenType.Not))
        {
            var line = _tokens[_current - 1].Line;
            var operand = ParseUnary();
            return new UnaryExpression(UnaryOperator.Not, operand, line);
        }

        return ParsePostfix();
    }

    private Expression ParsePostfix()
    {
        var expr = ParsePrimary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                var args = new List<Expression>();
                if (!Check(TokenType.RightParen))
                {
                    args.Add(ParseExpression());
                    while (Match(TokenType.Comma))
                    {
                        args.Add(ParseExpression());
                    }
                }
                Expect(TokenType.RightParen, ")");
                expr = new CallExpression(expr, args, expr.Line);

                continue;
            }

            if (Match(TokenType.Dot))
            {
                var fieldToken = Expect(TokenType.Identifier, "field name");
                expr = new FieldAccessExpression(expr, fieldToken.OriginalValue!, expr.Line);

                continue;
            }

            break;
        }

        return expr;
    }

    private ListExpression ParseListExpression()
    {
        var line = Current.Line;
        Expect(TokenType.LeftBracket, "[");

        var elements = new List<Expression>();
        if (!Check(TokenType.RightBracket))
        {
            elements.Add(ParseExpression());
            while (Match(TokenType.Comma))
            {
                elements.Add(ParseExpression());
            }
        }

        Expect(TokenType.RightBracket, "]");
        return new ListExpression(elements, line);
    }
}