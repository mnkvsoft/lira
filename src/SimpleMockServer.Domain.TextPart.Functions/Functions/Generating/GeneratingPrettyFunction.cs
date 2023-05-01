using SimpleMockServer.Domain.Generating;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;


internal class GeneratingFunction : ITextPart
{
    private readonly string? _format;
    private readonly IGeneratingFunction _function;

    public GeneratingFunction(IGeneratingFunction function, string? format)
    {
        _function = function;
        _format = format;
    }

    public string? Get(RequestData request) => _function.Generate(request).FormatIfFormattable(_format);
}

internal class GlobalGeneratingFunction : IGlobalTextPart
{
    private readonly string? _format;
    private readonly IGlobalGeneratingFunction _function;

    public GlobalGeneratingFunction(IGlobalGeneratingFunction function, string? format)
    {
        _function = function;
        _format = format;
    }

    public string? Get(RequestData _) => Get();

    public string? Get() => _function.Generate().FormatIfFormattable(_format);
}


internal static class FormattableHelper
{
    public static string? FormatIfFormattable(this object? value, string? format)
    {
        if (value is IFormattable formattable && format != null)
        {
            return formattable.ToString(format, null);
        }

        return value?.ToString();
    }
}
