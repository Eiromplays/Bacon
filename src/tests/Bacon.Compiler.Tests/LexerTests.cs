using Bacon.Compiler.Lexing;
using Shouldly;

namespace Bacon.Compiler.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_Integer_ProducesIntegerLiteralWithLongValue()
    {
        var tokens = Lexer.Tokenize("42");
        var first = tokens[0];

        first.Type.ShouldBe(TokenType.IntegerLiteral);
        first.Value.ShouldBe(42L);
    }

    [Fact]
    public void Tokenize_Decimal_ProducesDecimalLiteralWithDoubleValue()
    {
        var tokens = Lexer.Tokenize("3.14");
        var first = tokens[0];

        first.Type.ShouldBe(TokenType.DecimalLiteral);
        first.Value.ShouldBe(3.14);
    }

    [Fact]
    public void Tokenize_String_StripsQuotesFromValue()
    {
        var tokens = Lexer.Tokenize("\"hello\"");
        var first = tokens[0];

        first.Type.ShouldBe(TokenType.StringLiteral);
        first.Value.ShouldBe("hello");
        first.OriginalValue.ShouldBe("\"hello\"");  // Lexeme inkluderer hermetegn
    }

    [Fact]
    public void Tokenize_IntegerWithDotButNoDigit_DoesNotTreatAsDecimal()
    {
        // 5.foo skal være Integer(5), Dot, Identifier(foo) — ikke Decimal
        var tokens = Lexer.Tokenize("5.foo");

        tokens.Select(t => t.Type).ShouldBe([
            TokenType.IntegerLiteral,
            TokenType.Dot,
            TokenType.Identifier,
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_MultiLine_TracksLineNumbersCorrectly()
    {
        var source = "fast x\nfast y";
        var tokens = Lexer.Tokenize(source);

        // fast på linje 1, fast på linje 2
        tokens[0].Line.ShouldBe(1);
        tokens[2].Line.ShouldBe(2);
    }

    [Fact]
    public void Tokenize_TokenAfterWhitespace_HasCorrectColumn()
    {
        var tokens = Lexer.Tokenize("    fast");

        tokens[0].Type.ShouldBe(TokenType.Fast);
        tokens[0].Column.ShouldBe(5);  // 4 spaces, så fast starter på kolonne 5
    }

    [Fact]
    public void Tokenize_TokenStart_PointsToFirstCharacter()
    {
        // Ikke siste — det var den bug'en du fixet i går
        var tokens = Lexer.Tokenize("hello");

        tokens[0].Column.ShouldBe(1);  // h er på kolonne 1, ikke o (som ville vært slutten)
    }

    [Fact]
    public void Tokenize_UnterminatedString_ThrowsLexerException()
    {
        Should.Throw<LexerException>(() => Lexer.Tokenize("\"hello"));
    }

    [Fact]
    public void Tokenize_UnterminatedString_ExceptionContainsLineAndColumn()
    {
        var ex = Should.Throw<LexerException>(() => Lexer.Tokenize("\"hello"));

        ex.Line.ShouldBe(1);
        ex.Column.ShouldBe(1);  // strengen starter på posisjon 1
    }

    [Fact]
    public void Tokenize_UnknownCharacter_ProducesUnknownToken()
    {
        var tokens = Lexer.Tokenize("@");

        tokens[0].Type.ShouldBe(TokenType.Unknown);
    }

    [Fact]
    public void Tokenize_BesetningDeclaration_ProducesExpectedTokens()
    {
        var source = """
                     besetning Bil {
                         fast id : tekst
                     }
                     """;

        var types = Lexer.Tokenize(source).Select(t => t.Type).ToList();

        types.ShouldBe([
            TokenType.Besetning,
            TokenType.Identifier,    // Bil
            TokenType.LeftBrace,
            TokenType.Fast,
            TokenType.Identifier,    // id
            TokenType.Colon,
            TokenType.TextType,
            TokenType.RightBrace,
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_ErIkke_ProducesNotEqualToken()
    {
        var tokens = Lexer.Tokenize("x er ikke 5");

        tokens.Select(t => t.Type).ShouldBe([
            TokenType.Identifier,
            TokenType.NotEqual,
            TokenType.IntegerLiteral,
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_StorreEnn_ProducesGreaterThanToken()
    {
        var tokens = Lexer.Tokenize("x større enn 5");

        tokens.Select(t => t.Type).ShouldBe([
            TokenType.Identifier,
            TokenType.GreaterThan,
            TokenType.IntegerLiteral,
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_StorreEllerLik_ProducesGreaterOrEqualToken()
    {
        // Three-word match must take priority over two-word
        var tokens = Lexer.Tokenize("x større eller lik 5");

        tokens.Select(t => t.Type).ShouldBe([
            TokenType.Identifier,
            TokenType.GreaterOrEqual,
            TokenType.IntegerLiteral,
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_ForHver_ProducesForEachToken()
    {
        var tokens = Lexer.Tokenize("for hver x i liste");

        tokens.Select(t => t.Type).ShouldBe([
            TokenType.ForEach,
            TokenType.Identifier,
            TokenType.Identifier,    // i (soft keyword)
            TokenType.ListType,    // "liste" mappes til ListType
            TokenType.EndOfFile,
        ]);
    }

    [Fact]
    public void Tokenize_MergedToken_HasCorrectLexeme()
    {
        var tokens = Lexer.Tokenize("x er ikke 5");
        var notEqual = tokens[1];

        notEqual.OriginalValue.ShouldBe("er ikke");
    }

    [Fact]
    public void Tokenize_MergedToken_PreservesStartLineAndColumn()
    {
        var tokens = Lexer.Tokenize("x er ikke 5");
        var notEqual = tokens[1];

        notEqual.Line.ShouldBe(1);
        notEqual.Column.ShouldBe(3);  // "er" starter på kolonne 3
    }

    [Fact]
    public void Tokenize_IdentifierWithUnderscore_ProducesSingleIdentifier()
    {
        var tokens = Lexer.Tokenize("min_bil");

        tokens[0].Type.ShouldBe(TokenType.Identifier);
        tokens[0].OriginalValue.ShouldBe("min_bil");
    }

    [Fact]
    public void Tokenize_IdentifierStartingWithUnderscore_ProducesIdentifier()
    {
        var tokens = Lexer.Tokenize("_privat");

        tokens[0].Type.ShouldBe(TokenType.Identifier);
        tokens[0].OriginalValue.ShouldBe("_privat");
    }
}