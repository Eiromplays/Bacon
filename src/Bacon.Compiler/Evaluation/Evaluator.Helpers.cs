namespace Bacon.Compiler.Evaluation;

public sealed partial class Evaluator
{
    private static string TypeName(BaconValue value) => value switch
    {
        BaconInteger => "heltall",
        BaconDecimal => "desimal",
        BaconString => "tekst",
        BaconBoolean => "boolsk",
        BaconNothing => "ingenting",
        BaconList => "liste",
        BaconBesetningInstance b => b.TypeName,
        _ => "ukjent type"
    };

    private static string FormatValue(BaconValue value) => value switch
    {
        BaconInteger i => i.Value.ToString(),
        BaconDecimal d => d.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
        BaconString s => s.Value,
        BaconBoolean b => b.Value ? "sant" : "usant",
        BaconNothing => "ingenting",
        BaconList l => $"[{string.Join(", ", l.Elements.Select(FormatValue))}]",
        _ => value.ToString() ?? "?"
    };
}