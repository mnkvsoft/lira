namespace Lira.Domain;

public readonly struct RuleName
{
    public string Value { get; }

    public RuleName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }
}