using ArgValidation;

namespace Lira.Domain.Configuration.Parsing;

struct OperatorParameters
{
    private string Value { get; }

    public OperatorParameters(string value)
    {
        Arg.NotNullOrWhitespace(value, nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}