namespace Bacon.Compiler.Evaluation;

public sealed class Environment(Environment? parent = null)
{
    private readonly Dictionary<string, BaconValue> _bindings = new();
    private readonly HashSet<string> _immutable = [];

    public void Define(string name, BaconValue value, bool isImmutable = false)
    {
        _bindings[name] = value;
        if (isImmutable)
            _immutable.Add(name);
    }

    public BaconValue Get(string name)
    {
        if (_bindings.TryGetValue(name, out var value)) return value;

        return parent is not null ? parent.Get(name) : throw new RuntimeException($"Undefined variable: '{name}'");
    }

    public void Assign(string name, BaconValue value)
    {
        if (_bindings.ContainsKey(name))
        {
            if (_immutable.Contains(name))
                throw new RuntimeException($"Cannot reassign immutable variable: '{name}'");
            _bindings[name] = value;
            return;
        }

        if (parent is null)
            throw new RuntimeException($"Cannot assign to undefined variable: '{name}'");

        parent.Assign(name, value);
    }

    public bool IsDefined(string name)
    {
        if (_bindings.ContainsKey(name))
            return true;

        return parent is not null && parent.IsDefined(name);
    }
}