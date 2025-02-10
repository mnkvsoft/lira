using ArgValidation;

namespace Lira.Domain.Handling.Generating.Writers;

public class HeadersGenerator
{
    private readonly IReadOnlyCollection<GeneratingHeader> _headers;

    public HeadersGenerator(IReadOnlyCollection<GeneratingHeader> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    internal async Task<IReadOnlyCollection<Header>> Create(RuleExecutingContext context)
    {
        var result = new List<Header>();
        foreach (var header in _headers)
        {
            var value = await header.TextParts.Generate(context);
            result.Add(new Header(header.Name, value));
        }

        return result;
    }
}