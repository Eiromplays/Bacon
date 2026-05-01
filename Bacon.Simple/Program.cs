// See https://aka.ms/new-console-template for more information

using Bacon.Simple;

const string simpleBaconScript = "fast x er 5";
const string decimalBaconScript = "fast x er 5.53";
const string qouteBaconScript = """fast x er "hello" """;
const string commentTest = """
                           // dette er en kommentar
                           fast x er 5
                           // en til
                           fast y er 10
                           """;
const string testFull = """
                        prosess summer(a, b) {
                            leverer a + b
                        }
                        """;
const string fullTest = """
                        besetning Bil {
                            fast id : tekst
                            åpen modell : tekst
                        }

                        prosess hentVeteran(bil) {
                            leverer bil.årsmodell
                        }
                        """;

var tokens = Lexer.Tokenize(fullTest);

PrintTokens(tokens);
return;

static void PrintTokens(IEnumerable<Token> tokens)
{
    Console.WriteLine($"{"Line",-5} {"Col",-4} {"Type",-12} Value");

    foreach (var token in tokens)
    {
        Console.WriteLine(token.ToString());
    }
}