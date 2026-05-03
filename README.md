# Bacon

A toy programming language for building web APIs in Norwegian Bokmål.

Inspired by [Brunost](https://github.com/atomfinger/brunost) — a colleague
shared it, another colleague joked we should make our own called Bacon,
and I may or may not have taken it a little bit too seriously.

## Status

Bacon can now execute programs end-to-end with variables, functions, control
flow, and user-defined types. Web API support (route declarations bound to
ASP.NET endpoints) is planned but not yet implemented.

## Try it

```bash
dotnet run --project Bacon.Simple/Bacon.Simple.csproj examples/hello.bacon
dotnet run --project Bacon.Simple/Bacon.Simple.csproj examples/fakultet.bacon
dotnet run --project Bacon.Simple/Bacon.Simple.csproj examples/løkke.bacon
dotnet run --project Bacon.Simple/Bacon.Simple.csproj examples/bil.bacon
```

## Example

```bacon
besetning Bil {
    fast id : tekst
    åpen modell : tekst
    åpen årsmodell : heltall
}

prosess hovedprogram() {
    fast min_bil er Bil("1", "Volvo", 2018)
    skriv("Bil:", min_bil.modell, min_bil.årsmodell)
    
    min_bil.modell er "Toyota"
    min_bil.årsmodell er 2024
    skriv("Etter oppdatering:", min_bil.modell, min_bil.årsmodell)
}
```

Output:
Bil: Volvo 2018
Etter oppdatering: Tesla 2024

## What's done

- Lexer with multi-word operators (`større enn`, `er ikke`, etc.)
- Recursive descent parser with Pratt-style expression precedence
- AST nodes for the full language
- AstPrinter for debugging
- Tree-walker evaluator with lexical scoping and closures
- Functions with proper return semantics via ReturnException
- User-defined types via `besetning` with field access and assignment
- Builtin functions: `skriv`, `lengde`, `til_tekst`
- CLI for running `.bacon` files
- ~100 unit tests

## What's next

- Field immutability enforcement (`fast` fields blocking reassignment)
- Named constructor arguments (`Bil(id: "1", modell: "Volvo")`)
- More stdlib functions
- ASP.NET runtime: bind route declarations to actual web endpoints

## Project structure
src/
Bacon.Compiler/       Lexer, parser, AST, evaluator (class library)
Ast/
Lexing/
Parsing/
Evaluation/
Bacon.Simple/         CLI for running .bacon files
tests/
Bacon.Compiler.Tests/ xUnit tests
examples/               Sample Bacon programs

## Build

```bash
dotnet build
dotnet test
```