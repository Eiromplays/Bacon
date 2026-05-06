# Bacon Language Reference

Quick reference for all Bacon syntax and built-in functions.

## Variables

`fast` for immutable, `åpen` for mutable. Initial value required.

```bacon
fast x er 5
åpen y er "hello"
y er "world"
```

A name cannot be redeclared in the same scope, even from `fast` to `åpen` or vice versa. Reassigning an `åpen` variable uses `er` without `fast`/`åpen`.

## Scoping

A new scope is introduced by:

- A `prosess` body
- A `for hver` body (a fresh scope per iteration, so the loop variable does not leak out)
- A `rute` body

`hvis`/`ellers` and `så lenge` bodies share the enclosing scope. That means you cannot redeclare a name inside an `hvis` block that already exists outside it:

```bacon
fast x er 5
hvis sant {
    fast x er 10   // Runtime error: Cannot redefine immutable variable 'x'
}
```

Use a different name, or change the outer variable to `åpen` and reassign with `er`.

## Types

| Bacon | Description |
|-------|-------------|
| `heltall` | 64-bit integer |
| `desimal` | 64-bit floating point |
| `tekst` | String |
| `boolsk` | true/false (`sant`/`usant`) |
| `liste` | List |
| `ingenting` | Null/none value |

## Operators

### Arithmetic
`+`, `-`, `*`, `/`, `%`

Numeric promotion: integer + decimal = decimal.

### Comparison
- `er` — equality
- `er ikke` — inequality
- `større enn` — greater than
- `mindre enn` — less than
- `større eller lik` — greater or equal
- `mindre eller lik` — less or equal

Equality is value-based for primitives (`heltall`, `desimal`, `tekst`, `boolsk`, `ingenting`) and across the integer/decimal boundary. Lists and `besetning` instances currently compare by reference, so `[1, 2, 3] er [1, 2, 3]` returns `usant`. Comparing values of different types (e.g. `5 er "5"`) is always `usant`, never an error.

### Logical
- `og` — and
- `eller` — or
- `ikke` — not

## Control flow

### If/else

```bacon
hvis x større enn 5 {
    skriv("stor")
} ellers hvis x større enn 0 {
    skriv("liten men positiv")
} ellers {
    skriv("null eller negativ")
}
```

### While loop

```bacon
åpen i er 0
så lenge i mindre enn 10 {
    i er i + 1
}
```

### For each

```bacon
for hver tall i [1, 2, 3] {
    skriv(tall)
}
```

## Functions (prosess)

```bacon
prosess summer(a, b) {
    leverer a + b
}

prosess hovedprogram() {
    skriv(summer(2, 3))
}
```

`hovedprogram` is the entry point when running console programs.

## Custom types (besetning)

```bacon
besetning Bil {
    fast id : tekst        // immutable field
    åpen modell : tekst    // mutable field
}

prosess hovedprogram() {
    fast min_bil er Bil("1", "Volvo")
    skriv(min_bil.modell)
    min_bil.modell er "Toyota"
}
```

Constructor uses positional arguments, in declaration order. All fields are required.

## Errors

```bacon
prøv {
    kast "noe gikk galt"
} fanger feil {
    skriv("fanget:", feil)
}
```

(Note: try/catch is parsed but not yet evaluated)

## Built-in functions

| Function | Description |
|----------|-------------|
| `skriv(...)` | Print values to stdout, space-separated |
| `lengde(value)` | Length of string or list |
| `til_tekst(value)` | Convert any value to string |

## Web API

Routes are top-level declarations:

```bacon
rute GET "/bil/{id}" {
    leverer hent_bil(parameter.id)
}
```

Path parameters available as `parameter.<name>`.

Run as web server with `--serve`:

```bash
dotnet run --project src/Bacon.Cli/Bacon.Cli.csproj -- --serve api.bacon
```

Options:
- `--port <port>` — port to bind to (default 5000)
- `--host <host>` — host to bind to (default localhost; use `0.0.0.0` for container scenarios)

**Notes:**
- Bacon serves HTTP only. For HTTPS, use a reverse proxy like Caddy or nginx in front.
- HTTP methods are case-sensitive and must be uppercase. Lowercase `get` will be parsed as an identifier, not an HTTP method.

## Comments

```bacon
// Single-line comments use double-slash
```

## Norwegian-English glossary

| Bacon | English |
|-------|---------|
| `fast` | `let`/`const` (immutable) |
| `åpen` | `var`/`let` (mutable) |
| `prosess` | function |
| `besetning` | struct/record |
| `leverer` | return |
| `hvis` / `ellers` | if / else |
| `så lenge` | while |
| `for hver` | foreach |
| `er` | = (assignment) / == (equality) |
| `kast` | throw |
| `prøv` / `fanger` | try / catch |
| `sant` / `usant` | true / false |
| `ingenting` | null/none |
| `parameter` | path params (in routes) |