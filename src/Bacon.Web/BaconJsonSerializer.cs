using System.Text.Json;
using Bacon.Compiler.Evaluation;

namespace Bacon.Web;

public static class BaconJsonSerializer
{
    public static string Serialize(BaconValue value)
    {
        var obj = ToJsonCompatible(value);
        return JsonSerializer.Serialize(obj);
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