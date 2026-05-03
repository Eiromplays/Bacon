# Bacon

A toy programming language for building web APIs in Norwegian Bokmål.

Inspired by [Brunost](https://github.com/atomfinger/brunost) — a colleague
shared it, another colleague joked we should make our own called Bacon,
and I may or may not have taken it a little bit too seriously.

## Status

Bacon can now execute programs. Lexer, parser, and tree-walker evaluator are
complete. Web API support (route declarations bound to ASP.NET endpoints) is
planned but not yet implemented.

## Try it

```bash
dotnet run --project src/Bacon.Simple/Bacon.Simple.csproj examples/hello.bacon
dotnet run --project src/Bacon.Simple/Bacon.Simple.csproj examples/fakultet.bacon
dotnet run --project src/Bacon.Simple/Bacon.Simple.csproj examples/løkke.bacon
```

## Example

```bacon
prosess fakultet(n) {
    hvis n mindre eller lik 1 {
        leverer 1
    }
    leverer n * fakultet(n - 1)
}

prosess hovedprogram() {
    skriv("fakultet(5) =", fakultet(5))
}
```

Output:
fakultet(5) = 120

## What's done

- Lexer with multi-word operators (`større enn`, `er ikke`, etc.)
- Recursive descent parser with Pratt-style expression precedence
- AST nodes for the full language
- AstPrinter for debugging
- Tree-walker evaluator with lexical scoping and closures
- Functions with proper return semantics via ReturnException
- Builtin functions: `skriv`, `lengde`, `til_tekst`
- CLI for running `.bacon` files
- 93 unit tests

## What's next

- Besetning instances (creating and using struct-like values)
- Field assignment (`bil.modell er "Volvo"`)
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