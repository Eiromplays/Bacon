using System.Globalization;

namespace Bacon.Compiler.Lexing;

public sealed class Lexer
{
    private readonly string _source;
    private int _pos;
    private int _line = 1;
    private int _column = 1;
    private int _tokenStartLine;
    private int _tokenStartColumn;
    private int _tokenStartPos;
    private readonly List<Token> _tokens = [];

    private Lexer(string source) => _source = source;

    public static List<Token> Tokenize(string source)
    {
        var rawTokens = new Lexer(source).Run();
        return MultiWordOperatorMerger.Merge(rawTokens);
    }

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "fast", TokenType.Fast },
        { "er", TokenType.Is },
        { "åpen", TokenType.Open },
        { "prosess", TokenType.Process },
        { "hvis", TokenType.If },
        { "ellers", TokenType.Else },
        { "leverer", TokenType.Return },
        { "besetning", TokenType.Besetning },
        { "import", TokenType.Import },
        { "som", TokenType.As },
        { "rute", TokenType.Route },
        { "mottar", TokenType.Receives },
        { "med", TokenType.With },
        { "status", TokenType.Status },
        { "og", TokenType.And },
        { "eller", TokenType.Or },
        { "ikke", TokenType.Not },
        { "sant", TokenType.True },
        { "usant", TokenType.False },
        { "ingenting", TokenType.Nothing },
        { "tekst", TokenType.TextType },
        { "heltall", TokenType.IntegerType },
        { "desimal", TokenType.DecimalType },
        { "boolsk", TokenType.BooleanType },
        { "liste", TokenType.ListType },
        { "prøv", TokenType.Try },
        { "fanger", TokenType.Catch },
        { "kast", TokenType.Throw },
        { "gir", TokenType.Yield },
        { "GET", TokenType.HttpMethod },
        { "POST", TokenType.HttpMethod },
        { "PUT", TokenType.HttpMethod },
        { "DELETE", TokenType.HttpMethod },
        { "PATCH", TokenType.HttpMethod },

        { "for", TokenType.For },
        { "så", TokenType.Saa },
        { "større", TokenType.Greater },
        { "mindre", TokenType.Less },
        { "enn", TokenType.Than },
        { "hver", TokenType.Each },
        { "lenge", TokenType.Lenge },
        { "lik", TokenType.Equal },
    };

    private List<Token> Run()
    {
        while (!IsAtEnd())
        {
            if (char.IsWhiteSpace(Current))
            {
                Advance();
                continue;
            }

            if (Current == '/' && Peek() == '/')
            {
                AdvanceWhile(ch => ch != '\n');
                continue;
            }

            _tokenStartLine = _line;
            _tokenStartColumn = _column;
            _tokenStartPos = _pos;

            if (char.IsLetter(Current) || Current == '_')
            {
                AdvanceWhile(ch => char.IsLetterOrDigit(ch) || ch == '_');

                var word = _source.Substring(_tokenStartPos, _pos - _tokenStartPos);

                var tokenType = Keywords.GetValueOrDefault(word, TokenType.Identifier);

                AddToken(tokenType, word, tokenType == TokenType.Identifier ? word : null);

                continue;
            }

            if (char.IsDigit(Current))
            {
                AdvanceWhile(char.IsDigit);

                var isDecimal = false;
                if (Current == '.' && char.IsDigit(Peek()))
                {
                    Advance();
                    AdvanceWhile(char.IsDigit);

                    isDecimal = true;
                }

                var text = _source.Substring(_tokenStartPos, _pos - _tokenStartPos);

                if (isDecimal)
                    AddToken(TokenType.DecimalLiteral, text, double.Parse(text, CultureInfo.InvariantCulture));
                else
                    AddToken(TokenType.IntegerLiteral, text, long.Parse(text, CultureInfo.InvariantCulture));

                continue;
            }

            if (Current == '"')
            {
                Advance();
                var contentStart = _pos;
                AdvanceWhile(ch => ch != '"');

                if (IsAtEnd())
                {
                    throw new LexerException("Unterminated string", _tokenStartLine, _tokenStartColumn);
                }

                var contentEnd = _pos;
                Advance();

                var text = _source.Substring(contentStart, contentEnd - contentStart);

                AddToken(TokenType.StringLiteral, _source.Substring(_tokenStartPos, _pos - _tokenStartPos), text);

                continue;
            }

            switch (Current)
            {
                case '{':
                    AddSymbolToken(TokenType.LeftBrace);
                    continue;
                case '}':
                    AddSymbolToken(TokenType.RightBrace);
                    continue;
                case '(':
                    AddSymbolToken(TokenType.LeftParen);
                    continue;
                case ')':
                    AddSymbolToken(TokenType.RightParen);
                    continue;
                case ':':
                    AddSymbolToken(TokenType.Colon);
                    continue;
                case ',':
                    AddSymbolToken(TokenType.Comma);
                    continue;
                case '+':
                    AddSymbolToken(TokenType.Plus);
                    continue;
                case '-':
                    AddSymbolToken(TokenType.Minus);
                    continue;
                case '*':
                    AddSymbolToken(TokenType.Star);
                    continue;
                case '/':
                    AddSymbolToken(TokenType.Slash);
                    continue;
                case '%':
                    AddSymbolToken(TokenType.Percent);
                    continue;
                case '.':
                    AddSymbolToken(TokenType.Dot);
                    continue;
                case '[':
                    AddSymbolToken(TokenType.LeftBracket);
                    continue;
                case ']':
                    AddSymbolToken(TokenType.RightBracket);
                    continue;
            }

            AddSymbolToken(TokenType.Unknown);
        }

        AddToken(TokenType.EndOfFile, "", null);

        return _tokens;
    }

    private void AddToken(TokenType type, string? originalValue, object? value)
    {
        _tokens.Add(new Token(type, originalValue, value,
            _tokenStartLine, _tokenStartColumn, _tokenStartPos));
    }

    private void AddSymbolToken(TokenType type)
    {
        _tokens.Add(new Token(type, Current.ToString(), null,
            _tokenStartLine, _tokenStartColumn, _tokenStartPos));
        Advance();
    }

    private bool IsAtEnd() => _pos >= _source.Length;

    private char Current => IsAtEnd() ? '\0' : _source[_pos];

    private void Advance()
    {
        if (Current == '\n')
        {
            _line++;
            _column = 1;
        }
        else _column++;
        _pos++;
    }

    private void AdvanceWhile(Func<char, bool> predicate)
    {
        while (!IsAtEnd() && predicate(Current))
        {
            Advance();
        }
    }

    private char Peek() => _pos + 1 < _source.Length ? _source[_pos + 1] : '\0';
}