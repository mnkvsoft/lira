namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Echo : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "echo";
    public override bool ArgumentIsRequired => true;

    private object? _value;

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(_value);


    public override void SetArgument(string? argument)
    {
        _value = GetValue(argument);
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
