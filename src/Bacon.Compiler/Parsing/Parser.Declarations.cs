using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;

namespace Bacon.Compiler.Parsing;

public sealed partial class Parser
{
     private Declaration ParseDeclaration()
    {
        return Current.Type switch
        {
            TokenType.Import => ParseImportDeclaration(),
            TokenType.Besetning => ParseBesetningDeclaration(),
            TokenType.Process => ParseProcessDeclaration(),
            TokenType.Route => ParseRouteDeclaration(),
            _ => throw new ParseException(
                $"Expected declaration (import/besetning/prosess/rute), got '{Current.OriginalValue}'",
                Current.Line, Current.Column)
        };
    }

    // import drift.tjener
    // import drift.tjener som t
    private ImportDeclaration ParseImportDeclaration()
    {
        var token = Expect(TokenType.Import, "import");

        // Build path: identifier (. identifier)*
        var pathParts = new List<string>
        {
            Expect(TokenType.Identifier, "module name").OriginalValue!
        };

        while (Match(TokenType.Dot))
        {
            pathParts.Add(Expect(TokenType.Identifier, "submodule name").OriginalValue!);
        }

        var path = string.Join(".", pathParts);

        // Optional alias: "som x"
        string? alias = null;
        if (Match(TokenType.As))
        {
            alias = Expect(TokenType.Identifier, "alias name").OriginalValue;
        }

        return new ImportDeclaration(path, alias, token.Line);
    }

    // besetning Bil { fast id : tekst, åpen modell : tekst }
    private BesetningDeclaration ParseBesetningDeclaration()
    {
        var token = Expect(TokenType.Besetning, "besetning");
        var name = Expect(TokenType.Identifier, "besetning name");

        Expect(TokenType.LeftBrace, "{");

        var fields = new List<FieldDeclaration>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            fields.Add(ParseFieldDeclaration());

            // Optional comma between fields
            Match(TokenType.Comma);  // consume if it is there, else skip
        }

        Expect(TokenType.RightBrace, "}");

        return new BesetningDeclaration(name.OriginalValue!, fields, token.Line);
    }

    private FieldDeclaration ParseFieldDeclaration()
    {
        var firstToken = Current;

        // fast eller åpen
        bool isImmutable;
        if (Match(TokenType.Fast))
            isImmutable = true;
        else if (Match(TokenType.Open))
            isImmutable = false;
        else
            throw new ParseException(
                $"Expected 'fast' or 'åpen', got '{Current.OriginalValue}'",
                Current.Line, Current.Column);

        var name = Expect(TokenType.Identifier, "field name");
        Expect(TokenType.Colon, ":");

        // Type name can be a type-keyword or an identifier
        var typeName = ParseTypeName();

        return new FieldDeclaration(
            name.OriginalValue!,
            typeName,
            isImmutable,
            firstToken.Line);
    }

    private string ParseTypeName()
    {
        var token = Current;

        if (token.Type is not (TokenType.TextType or TokenType.IntegerType or TokenType.DecimalType
            or TokenType.BooleanType or TokenType.ListType or TokenType.Identifier))
            throw new ParseException(
                $"Expected type name, got '{token.OriginalValue}'",
                token.Line, token.Column);

        Advance();
        return token.OriginalValue!;
    }

    // prosess summer(a, b) { ... }
    // prosess summer(a : heltall, b : heltall) { ... }
    private ProcessDeclaration ParseProcessDeclaration()
    {
        var token = Expect(TokenType.Process, "prosess");
        var name = Expect(TokenType.Identifier, "process name");

        Expect(TokenType.LeftParen, "(");

        var parameters = new List<ParameterDeclaration>();
        if (!Check(TokenType.RightParen))
        {
            parameters.Add(ParseParameterDeclaration());
            while (Match(TokenType.Comma))
            {
                parameters.Add(ParseParameterDeclaration());
            }
        }

        Expect(TokenType.RightParen, ")");

        var body = ParseBlock();

        return new ProcessDeclaration(
            name.OriginalValue!,
            parameters,
            body,
            token.Line);
    }

    private ParameterDeclaration ParseParameterDeclaration()
    {
        var nameToken = Expect(TokenType.Identifier, "parameter name");

        // Optional type-annotation: "name : type"
        string? typeName = null;
        if (Match(TokenType.Colon))
        {
            typeName = ParseTypeName();
        }

        return new ParameterDeclaration(
            nameToken.OriginalValue!,
            typeName,
            nameToken.Line);
    }

    // rute GET "/bil/{id}" { ... }
    // rute POST "/bil" mottar data : NyBil { ... }
    private RouteDeclaration ParseRouteDeclaration()
    {
        var token = Expect(TokenType.Route, "rute");
        var methodToken = Expect(TokenType.HttpMethod, "HTTP method (GET, POST, ...)");
        var pathToken = Expect(TokenType.StringLiteral, "route path string");

        // Optional body binding: "mottar X : Type"
        BodyBinding? bodyBinding = null;
        if (Match(TokenType.Receives))
        {
            var bindingNameToken = Expect(TokenType.Identifier, "body binding name");
            Expect(TokenType.Colon, ":");
            var typeName = ParseTypeName();
            bodyBinding = new BodyBinding(
                bindingNameToken.OriginalValue!,
                typeName,
                bindingNameToken.Line);
        }

        var body = ParseBlock();

        return new RouteDeclaration(
            methodToken.OriginalValue!,
            (string)pathToken.Value!,
            bodyBinding,
            body,
            token.Line);
    }
}