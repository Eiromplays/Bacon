using System.Globalization;

namespace Bacon.Compiler.Lexing;

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
    With,
    Status,
    And,
    Or,
    Not,
    True,
    False,
    Nothing,
    TextType,
    IntegerType,
    DecimalType,
    BooleanType,
    ListType,
    Try,
    Catch,
    Throw,
    Yield,
    HttpMethod,

    // Multi-word operator components
    For,
    Saa,
    Greater,
    Less,
    Than,
    Each,
    Lenge,
    Equal,

    // Multi-word operators (final tokens after merging)
    ForEach,
    While,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    NotEqual,

    // Literals
    Identifier,
    IntegerLiteral,
    DecimalLiteral,
    StringLiteral,

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