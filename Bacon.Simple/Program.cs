using Bacon.Compiler.Evaluation;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;

if (args.Length < 1)
{
    Console.Error.WriteLine("Bruk: bacon <fil.bacon>");
    return 1;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"Fil ikke funnet: {filePath}");
    return 1;
}

var source = await File.ReadAllTextAsync(filePath);

try
{
    var tokens = Lexer.Tokenize(source);
    var program = Parser.Parse(tokens);
    Evaluator.Evaluate(program);
    return 0;
}
catch (LexerException ex)
{
    Console.Error.WriteLine($"Lexer-feil: {ex.Message}");
    return 1;
}
catch (ParseException ex)
{
    Console.Error.WriteLine($"Parser-feil: {ex.Message}");
    return 1;
}
catch (RuntimeException ex)
{
    Console.Error.WriteLine($"Runtime-feil: {ex.Message}");
    return 1;
}