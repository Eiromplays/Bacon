# Bacon

A toy programming language for building web APIs in Norwegian Bokmål.

Inspired by [Brunost](https://github.com/atomfinger/brunost) — a colleague
shared it, another colleague joked we should make our own called Bacon,
and I may or may not have taken it a little bit too seriously.

## Status

Frontend complete: lexer + parser + AST. Can parse full Bacon syntax
including processes, besetninger, routes, and expressions with proper
operator precedence. Evaluator and runtime: TBD.

## Example

```bacon
import drift.tjener
import drift.register

besetning Bil {
    fast id : tekst
    åpen modell : tekst
    åpen årsmodell : heltall
}

prosess erVeteran(bil) {
    leverer bil.årsmodell mindre enn 1990
}

rute GET "/bil/{id}" {
    leverer register.hent(Bil)
}
```

## What's done

- Lexer with 50+ token types
- Multi-word operators (`større enn`, `er ikke`, etc.)
- Line and column tracking for error messages
- AST nodes for the full language
- Recursive descent parser with Pratt-style expression parsing
- 50+ unit tests covering lexer and parser

## What's next

- Tree-walker evaluator (executes parsed programs)
- Standard library stubs (`terminal.skriv` etc.)
- ASP.NET runtime that maps route declarations to actual endpoints
- CLI tool: `bacon run program.bacon`

## Project structure
src/
Bacon.Compiler/       Lexer, parser, AST (class library)
Lexing/
Parsing/
Ast/
Bacon.Simple/         Console playground for testing
tests/
Bacon.Compiler.Tests/ xUnit tests

## Build

```bash
dotnet build
dotnet test
```