using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;
internal class GeneratingPrettyFunction : IGeneratingFunction
{
    private readonly string? _format;
    private readonly IGeneratingPrettyFunction _function;

    public GeneratingPrettyFunction(IGeneratingPrettyFunction function, string? format)
    {
        _function = function;
        _format = format;
    }

    public string? Generate(RequestData request)
    {
        object? value = _function.Generate(request);
        if (value is IFormattable formattable && _format != null)
        {
            return formattable.ToString(_format, null);
        }

        return value?.ToString();
    }
}

