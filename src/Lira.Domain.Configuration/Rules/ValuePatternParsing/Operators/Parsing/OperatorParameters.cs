namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

public enum OperatorParametersType
{
    SingleLine,
    Full
}

class OperatorParameters
{
    public OperatorParametersType Type { get; }
    public string Value { get; }

    public OperatorParameters(OperatorParametersType type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
        Type = type;
    }

    public override string ToString() => Value;
}