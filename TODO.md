# TODO

Items found during code review with Claude Cowork (2026-05-03).

## Bugs to fix

All bugs from initial code review have been fixed.

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

## Future features

- Status codes (`leverer status 404`)
- Body binding (`mottar data : NyBil`)
- Query parameters
- Named constructor arguments (`Bil(id: "1", modell: "Volvo")`)
- String escapes (`\n`, `\t`)
- More stdlib functions
- Package as `dotnet tool` for direct `bacon` command
- Module system (currently `import` is a no-op)
