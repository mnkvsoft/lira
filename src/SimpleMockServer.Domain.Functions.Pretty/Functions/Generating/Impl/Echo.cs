using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl;

internal class Echo : IGeneratingPrettyFunction, IWithStringArgumenFunction
{
    public static string Name => "echo";

    private object? _value;

    public object? Generate(HttpRequest request)
    {
        return _value;
    }

    public void SetArgument(string? argument)
    {
        _value = GetValue(argument);
    }

    private object? GetValue(string? argument)
    {
        if (System.Guid.TryParse(argument, out var guid))
            return guid;
        if (long.TryParse(argument, out var value))
            return value;
        if (decimal.TryParse(argument, out var decimalValue))
            return decimalValue;

        return argument;
    }
}
