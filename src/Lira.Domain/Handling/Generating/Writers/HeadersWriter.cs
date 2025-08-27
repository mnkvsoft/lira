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

    internal IReadOnlyCollection<Header> Create(RuleExecutingContext context)
    {
        var result = new List<Header>();
        foreach (var header in _headers)
        {
            var value = header.TextPartsProvider.GetSingleString(context);
            result.Add(new Header(header.Name, value));
        }

        return result;
    }
}