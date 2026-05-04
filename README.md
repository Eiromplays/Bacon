# Bacon

A toy programming language for building web APIs in Norwegian Bokmål.

Inspired by [Brunost](https://github.com/atomfinger/brunost) which a colleague
shared. Another colleague joked we should make our own called Bacon,
and I may or may not have taken it a little bit too seriously.

## Status

Bacon is a working programming language with web API support. Programs run as
console apps or as HTTP servers serving JSON APIs.

## Try a console program

```bash
dotnet run --project src/Bacon.Cli/Bacon.Cli.csproj examples/hello.bacon
dotnet run --project src/Bacon.Cli/Bacon.Cli.csproj examples/fakultet.bacon
dotnet run --project src/Bacon.Cli/Bacon.Cli.csproj examples/bil.bacon
```

## Try the web API
```bash
dotnet run --project src/Bacon.Cli/Bacon.Cli.csproj -- --serve examples/bil_api.bacon
```

In another terminal:
```bash
curl http://localhost:5000/bil/123
# {"id":"123","modell":"Volvo V40","årsmodell":2018}
```

## Example: web API

```bacon
besetning Bil {
    fast id : tekst
    fast modell : tekst
    fast årsmodell : heltall
}

prosess hent_bil(bil_id) {
    leverer Bil(bil_id, "Volvo V40", 2018)
}

rute GET "/bil/{id}" {
    leverer hent_bil(parameter.id)
}
```

## Documentation

See [LANGUAGE.md](LANGUAGE.md) for full syntax reference.

## What's done

- Lexer with multi-word operators (`større enn`, `er ikke`, etc.)
- Recursive descent parser with Pratt-style expression precedence
- AST nodes for the full language
- AstPrinter for debugging
- Tree-walker evaluator with lexical scoping and closures
- User-defined types (`besetning`) with field immutability
- Builtin functions: `skriv`, `lengde`, `til_tekst`
- ASP.NET web runtime with route mapping and JSON responses
- Path parameters via `parameter.x` syntax
- CLI for running and serving `.bacon` files
- 100+ unit tests

## What's next

- Status codes (`leverer status 404`)
- Body binding (`mottar data : NyBil`)
- Query parameters
- Named constructor arguments (`Bil(id: "1", modell: "Volvo")`)
- String escapes (`\n`, `\t`)
- More stdlib functions

## Project structure

```
src/
    Bacon.Compiler/       Lexer, parser, AST, evaluator
    Bacon.Web/            ASP.NET integration
    Bacon.Cli/            CLI for running and serving .bacon files
    tests/
        Bacon.Compiler.Tests/ xUnit tests
examples/               Sample Bacon programs
```

## Build

```bash
dotnet build
dotnet test
```