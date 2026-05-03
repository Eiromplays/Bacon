using Bacon.Compiler.Evaluation;
using Bacon.Compiler.Lexing;
using Bacon.Compiler.Parsing;
using Bacon.Web;

if (args.Length < 1)
{
    PrintUsage();
    return 1;
}

var serve = args.Contains("--serve");
var portArg = ParsePortArg(args);
var filePath = ParseFilePath(args);

if (portArg.HasValue && !serve)
{
    Console.Error.WriteLine("Error: --port can only be used with --serve");
    PrintUsage();
    return 1;
}

if (filePath == null)
{
    Console.Error.WriteLine("Error: missing file argument");
    PrintUsage();
    return 1;
}

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"File not found: {filePath}");
    return 1;
}

var port = portArg ?? 5000;
var source = await File.ReadAllTextAsync(filePath);

try
{
    var tokens = Lexer.Tokenize(source);
    var program = Parser.Parse(tokens);

    if (serve)
    {
        var host = new BaconWebHost(program);
        await host.RunAsync(port);
    }
    else
    {
        Evaluator.Evaluate(program);
    }

    return 0;
}
catch (LexerException ex)
{
    Console.Error.WriteLine($"Lexer error: {ex.Message}");
    return 1;
}
catch (ParseException ex)
{
    Console.Error.WriteLine($"Parser error: {ex.Message}");
    return 1;
}
catch (RuntimeException ex)
{
    Console.Error.WriteLine($"Runtime error: {ex.Message}");
    return 1;
}

static int? ParsePortArg(string[] args)
{
    var index = Array.IndexOf(args, "--port");
    if (index < 0) return null;

    if (index + 1 >= args.Length)
        throw new ArgumentException("--port requires a value");

    var portStr = args[index + 1];
    if (!int.TryParse(portStr, out var port))
        throw new ArgumentException($"Invalid port value: '{portStr}' is not a number");

    return port is < 1 or > 65535 ? throw new ArgumentException($"Invalid port value: {port} (must be 1-65535)") : port;
}

static string? ParseFilePath(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--"))
            return args[i];
        if (args[i] == "--port")
            i++;
    }
    return null;
}

static void PrintUsage()
{
    Console.Error.WriteLine("Usage: Bacon.Cli [--serve] [--port <port>] <file.bacon>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Options:");
    Console.Error.WriteLine("  --serve         Run as a web server");
    Console.Error.WriteLine("  --port <port>   Port to serve on (default 5000, requires --serve)");
}