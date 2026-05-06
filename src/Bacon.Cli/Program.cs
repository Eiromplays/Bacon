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

int? portArg;
string? hostArg;
try
{
    portArg = ParsePortArg(args);
    hostArg = ParseHostArg(args);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    PrintUsage();
    return 1;
}

var filePath = ParseFilePath(args);

if ((portArg.HasValue || hostArg != null) && !serve)
{
    Console.Error.WriteLine("Error: --port and --host can only be used with --serve");
    PrintUsage();
    return 1;
}

if (string.IsNullOrWhiteSpace(filePath))
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

var source = await File.ReadAllTextAsync(filePath);

try
{
    var tokens = Lexer.Tokenize(source);
    var program = Parser.Parse(tokens);

    if (serve)
    {
        var host = new BaconWebHost(program);
        await host.RunAsync(portArg ?? 5000, hostArg ?? "localhost");
    }
    else
    {
        Evaluator.Evaluate(program);
    }

    return 0;
}
catch (LexerException ex)
{
    Console.Error.WriteLine($"Lexer error at {filePath}:{ex.Line}:{ex.Column}: {ex.Message}");
    return 1;
}
catch (ParseException ex)
{
    Console.Error.WriteLine($"Parser error at {filePath}:{ex.Line}:{ex.Column}: {ex.Message}");
    return 1;
}
catch (RuntimeException ex)
{
    Console.Error.WriteLine(ex.Line.HasValue
        ? $"Runtime error at {filePath}:{ex.Line}: {ex.Message}"
        : $"Runtime error: {ex.Message}");
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

static string? ParseHostArg(string[] args)
{
    var index = Array.IndexOf(args, "--host");
    if (index < 0) return null;

    return index + 1 >= args.Length ? throw new ArgumentException("--host requires a value") : args[index + 1];
}

static string? ParseFilePath(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal))
            return args[i];

        if (args[i] is "--port" or "--host")
            i++;
    }
    return null;
}

static void PrintUsage()
{
    Console.Error.WriteLine("Usage: Bacon.Cli [--serve] [--port <port>] [--host <host>] <file.bacon>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Options:");
    Console.Error.WriteLine("  --serve         Run as a web server");
    Console.Error.WriteLine("  --port <port>   Port to serve on (default 5000, requires --serve)");
    Console.Error.WriteLine("  --host <host>   Host to bind to (default localhost, requires --serve)");
}