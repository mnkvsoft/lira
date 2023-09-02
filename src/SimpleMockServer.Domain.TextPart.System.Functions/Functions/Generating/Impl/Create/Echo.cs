namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating.Impl.Create;

internal class Echo : IGlobalObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "echo";

    private object? _value;

    public object? Get(RequestData request) => Get();
    public object? Get() => _value;

    public void SetArgument(string? argument)
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
