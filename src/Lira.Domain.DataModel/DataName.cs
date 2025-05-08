using ArgValidation;

namespace Lira.Domain.DataModel;

public readonly record struct DataName
{
    private readonly string _value;

    public DataName(string value)
    {
        Arg.Validate(value, nameof(value))
            .NotNullOrWhitespace();

        _value = value.ToLower();
    }
    public override string ToString()
    {
        return _value;
    }
}
