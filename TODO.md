# TODO

Items found during code review with Claude Cowork (2026-05-03).

## Bugs to fix

- [ ] Immutable variables can be silently redefined in `Environment.Define` (allows `fast x er 5; fast x er 10`)
- [ ] `for hver` loop variable leaks into surrounding scope and can clobber outer `fast` bindings
- [ ] Numeric literal overflow throws raw `OverflowException`/`FormatException` instead of `LexerException`
- [ ] Unknown character emits an `Unknown` token instead of throwing `LexerException` immediately
- [ ] CLI swallows source location in error output (only shows `ex.Message`, not line/column)
- [ ] Web pipeline doesn't catch unexpected exceptions; non-`RuntimeException` leaks stack trace to client
- [ ] Assignment to non-target (e.g. `5 er 10`, `f() er 10`) parses without complaint, fails only at evaluation

## Code quality

- [ ] Magic precedence number `31` in `Parser.Statements.cs` — compute as `BinaryOperators[TokenType.Is].Precedence + 1`
- [ ] `AreEqual` returns false for structurally-identical lists/besetnings — extend to compare elements/fields
- [ ] `BaconProcess.Closure` always captures `_global`; rename or document that nested process declarations aren't supported
- [ ] HTTP method tokens are case-sensitive in lexer; either fold to uppercase or document explicitly
- [ ] `BaconWebHost` always binds to localhost; consider `--host` flag for container scenarios
- [ ] `RuntimeException` carries no source location; add `(string message, int line)` constructor
- [ ] Magic string `"parameter"` in `Evaluator.Web.cs` should be a named constant
- [ ] `Token.ToString()` drops `OriginalValue` for tokens with null `Value`
- [ ] CA1859 suppressions are scattered across files; consolidate via `.editorconfig`
- [ ] `WriteJsonResponse` mixes manual content-type with `WriteAsJsonAsync` style elsewhere

## OSS readiness

- [ ] Add LICENSE (MIT)
- [ ] Add GitHub Actions workflow for `dotnet build && dotnet test`
- [ ] Verify all `examples/*.bacon` files are committed
- [ ] Consider package metadata in csproj files (`<Authors>`, `<Description>`, `<RepositoryUrl>`)
- [ ] Add language glossary to README (prosess = function, besetning = struct, fast/åpen = let/var)

## Future features

- Status codes (`leverer status 404`)
- Body binding (`mottar data : NyBil`)
- Query parameters
- Named constructor arguments (`Bil(id: "1", modell: "Volvo")`)
- String escapes (`\n`, `\t`)
- More stdlib functions
- Package as `dotnet tool` for direct `bacon` command
- Module system (currently `import` is a no-op)