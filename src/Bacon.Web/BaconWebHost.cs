using Bacon.Compiler.Ast;
using Bacon.Compiler.Evaluation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Bacon.Web;

public sealed class BaconWebHost
{
    private readonly Program _program;
    private readonly Evaluator _evaluator;

    public BaconWebHost(Program program)
    {
        _program = program;
        _evaluator = new Evaluator();
        _evaluator.PrepareGlobalScope(program);
    }

    public async Task RunAsync(int port = 5000, string host = "localhost")
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        foreach (var decl in _program.Declarations)
        {
            if (decl is RouteDeclaration route)
            {
                RegisterRoute(app, route);
            }
        }

        Console.WriteLine($"Bacon serving on http://{host}:{port}");
        await app.RunAsync($"http://{host}:{port}");
    }

    private void RegisterRoute(WebApplication app, RouteDeclaration route)
    {
        var handler = (HttpContext context) => HandleRequest(context, route);

        switch (route.HttpMethod.ToUpperInvariant())
        {
            case "GET":
                app.MapGet(route.Path, handler);
                break;
            case "POST":
                app.MapPost(route.Path, handler);
                break;
            case "PUT":
                app.MapPut(route.Path, handler);
                break;
            case "DELETE":
                app.MapDelete(route.Path, handler);
                break;
            case "PATCH":
                app.MapPatch(route.Path, handler);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported HTTP method: {route.HttpMethod}");
        }
    }

    private async Task HandleRequest(HttpContext context, RouteDeclaration route)
    {
        var pathParams = context.Request.RouteValues
            .Where(kv => kv.Value is string)
            .ToDictionary(kv => kv.Key, kv => (string)kv.Value!);

        try
        {
            var result = _evaluator.EvaluateRoute(route, pathParams);
            await WriteJsonResponse(context, result);
        }
        catch (RuntimeException ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message }, BaconJsonSerializer.Options);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected errors (NullRef, leaking ReturnException, etc.)
            // Don't leak internal details to the client
            await Console.Error.WriteLineAsync($"Internal error: {ex}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error" }, BaconJsonSerializer.Options);
        }
    }

    private static async Task WriteJsonResponse(HttpContext context, BaconValue value)
    {
        var json = BaconJsonSerializer.Serialize(value);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
}