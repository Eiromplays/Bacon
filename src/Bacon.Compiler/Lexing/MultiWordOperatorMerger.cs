namespace Bacon.Compiler.Lexing;

public static class MultiWordOperatorMerger
{
    private static readonly Dictionary<(TokenType, TokenType, TokenType), TokenType> ThreeWordOperators = new()
    {
        [(TokenType.Greater, TokenType.Or, TokenType.Equal)] = TokenType.GreaterOrEqual,
        [(TokenType.Less, TokenType.Or, TokenType.Equal)] = TokenType.LessOrEqual,
    };

    private static readonly Dictionary<(TokenType, TokenType), TokenType> TwoWordOperators = new()
    {
        [(TokenType.Greater, TokenType.Than)] = TokenType.GreaterThan,
        [(TokenType.Less, TokenType.Than)] = TokenType.LessThan,
        [(TokenType.Is, TokenType.Not)] = TokenType.NotEqual,
        [(TokenType.For, TokenType.Each)] = TokenType.ForEach,
        [(TokenType.Saa, TokenType.Lenge)] = TokenType.While,
    };

    public static List<Token> Merge(IReadOnlyList<Token> tokens)
    {
        var result = new List<Token>();
        var i = 0;

        while (i < tokens.Count)
        {
            if (i + 2 < tokens.Count)
            {
                var threeKey = (tokens[i].Type, tokens[i + 1].Type, tokens[i + 2].Type);

                if (ThreeWordOperators.TryGetValue(threeKey, out var threeType))
                {
                    var first = tokens[i];
                    var lexeme = $"{tokens[i].OriginalValue} {tokens[i + 1].OriginalValue} {tokens[i + 2].OriginalValue}";
                    result.Add(first with { Type = threeType, OriginalValue = lexeme, Value = null });
                    i += 3;

                    continue;
                }
            }

            if (i + 1 < tokens.Count)
            {
                var twoKey = (tokens[i].Type, tokens[i + 1].Type);

                if (TwoWordOperators.TryGetValue(twoKey, out var twoType))
                {
                    var first = tokens[i];
                    var lexeme = $"{tokens[i].OriginalValue} {tokens[i + 1].OriginalValue}";
                    result.Add(first with { Type = twoType, OriginalValue = lexeme, Value = null });
                    i += 2;

                    continue;
                }
            }

            result.Add(tokens[i]);
            i++;
        }

        return result;
    }
}