using System.Globalization;

namespace Bacon.Simple;

public enum TokenType
{
    // Keywords
    Fast,
    Open,
    Is,
    Process,
    Return,
    If,
    Else,
    Besetning,
    Import,
    As,
    Route,
    Receives,
    PathParam,
    QueryParam,
    With,
    Status,
    And,
    Or,
    Not,
    True,
    False,
    Nothing,
    Text,
    Integer,
    Decimal,
    Boolean,
    List,
    In,
    Try,
    Catch,
    Throw,
    Yield,
    HttpMethod,


    // Literals
    Identifier,
    IntegerLiteral,
    DecimalLiteral,
    String,

    // Symbols
    LeftBrace, RightBrace,
    LeftParen, RightParen,
    LeftBracket, RightBracket,
    Colon,
    Comma,
    Dot,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    // Special
    Unknown,
    EndOfFile,
}

public sealed record Token(TokenType Type, string? OriginalValue, object? Value, int Line, int Column, int Position)
{
    public override string ToString() => $"{Line,-5} {Column,-4} {Type,-12} {(Value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : Value?.ToString() ?? "")}";
}