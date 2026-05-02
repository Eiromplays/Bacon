using Bacon.Compiler.Ast;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;

const string source = """
                      prosess sjekkAlder(person) {
                          hvis person.alder større eller lik 18 {
                              leverer "voksen"
                          } ellers hvis person.alder større enn 12 {
                              leverer "tenåring"
                          } ellers {
                              leverer "barn"
                          }
                      }
                      """;

var tokens = Lexer.Tokenize(source);
var program = Parser.Parse(tokens);
Console.WriteLine(AstPrinter.Print(program));