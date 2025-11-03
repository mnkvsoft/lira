namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Echo : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "echo";
    public override bool ArgumentIsRequired => true;

    private object? _value;

    public dynamic? Get(RuleExecutingContext context) => _value;

    public Type Type => DotNetType.Unknown;


    public override void SetArgument(string? arguments)
    {
        _value = GetValue(arguments);
    }

    private object? GetValue(string? argument)
    {
        if (global::System.Guid.TryParse(argument, out var guid))
            return guid;
        if (long.TryParse(argument, out var value))
            return value;
        if (decimal.TryParse(argument, out var decimalValue))
            return decimalValue;

        return argument;
    }
}
