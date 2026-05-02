using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;

const string fullExample = """
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
                           """;

var tokens = Lexer.Tokenize(fullExample);
var program = Parser.Parse(tokens);

Console.WriteLine($"Parsed {program.Declarations.Count} declarations");
foreach (var decl in program.Declarations)
{
    Console.WriteLine($"  {decl.GetType().Name} at line {decl.Line}");
}