namespace Bacon.Compiler.Ast;

// import drift.tjener
// import drift.tjener som t
public sealed record ImportDeclaration(string Path, string? Alias, int Line) : Declaration(Line);

// besetning Bil { fast id : tekst, åpen modell : tekst }
public sealed record BesetningDeclaration(
    string Name,
    IReadOnlyList<FieldDeclaration> Fields,
    int Line) : Declaration(Line);

// Et felt inni en besetning
public sealed record FieldDeclaration(
    string Name,
    string TypeName,            // "tekst", "heltall", etc.
    bool IsImmutable,           // fast vs åpen
    int Line) : AstNode(Line);

// prosess summer(a, b) { ... }
public sealed record ProcessDeclaration(
    string Name,
    IReadOnlyList<ParameterDeclaration> Parameters,
    IReadOnlyList<Statement> Body,
    int Line) : Declaration(Line);

public sealed record ParameterDeclaration(
    string Name,
    string? TypeName,          // type-annotering er valgfri
    int Line) : AstNode(Line);

// rute GET "/bil/{id}" mottar data : NyBil { ... }
public sealed record RouteDeclaration(
    string HttpMethod,                       // "GET", "POST", etc.
    string Path,                              // "/bil/{id}"
    BodyBinding? Body,                        // mottar data : NyBil (valgfri)
    IReadOnlyList<Statement> Statements,
    int Line) : Declaration(Line);

public sealed record BodyBinding(
    string Name,
    string TypeName,
    int Line) : AstNode(Line);