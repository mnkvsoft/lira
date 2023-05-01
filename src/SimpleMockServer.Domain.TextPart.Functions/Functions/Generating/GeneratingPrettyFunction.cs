using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
internal class GeneratingPrettyFunction : ITextPart
{
    private readonly string? _format;
    private readonly IGeneratingPrettyFunction _function;

    public GeneratingPrettyFunction(IGeneratingPrettyFunction function, string? format)
    {
        _function = function;
        _format = format;
    }

    public string? Get(RequestData request)
    {
        var value = _function.Generate(request);
        if (value is IFormattable formattable && _format != null)
        {
            return formattable.ToString(_format, null);
        }

        return value?.ToString();
    }
}

