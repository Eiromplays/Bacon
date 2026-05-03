using System.Text.Encodings.Web;
using System.Text.Json;
using Bacon.Compiler.Evaluation;

namespace Bacon.Web;

public static class BaconJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        // UnsafeRelaxedJsonEscaping is safe for API responses with application/json content-type.
        // It allows non-ASCII characters (like Norwegian å, ø, æ) to be sent as UTF-8 directly
        // instead of being escaped as \u00e5 etc. The "unsafe" prefix refers to potential issues
        // when JSON is embedded in HTML, which doesn't apply to API responses.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize(BaconValue value)
    {
        var obj = ToJsonCompatible(value);
        return JsonSerializer.Serialize(obj, Options);
    }

    private static object? ToJsonCompatible(BaconValue value) => value switch
    {
        BaconInteger i => i.Value,
        BaconDecimal d => d.Value,
        BaconString s => s.Value,
        BaconBoolean b => b.Value,
        BaconNothing => null,
        BaconList list => list.Elements.Select(ToJsonCompatible).ToList(),
        BaconBesetningInstance instance => instance.Fields
            .ToDictionary(f => f.Key, f => ToJsonCompatible(f.Value)),
        _ => value.ToString()
    };
}