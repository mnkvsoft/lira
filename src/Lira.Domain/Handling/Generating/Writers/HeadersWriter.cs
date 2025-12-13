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

    internal IEnumerable<Header> Create(RuleExecutingContext context)
    {
        foreach (var header in _headers)
        {
            var value = header.TextPartsProvider.GetSingleString(context);
            yield return new Header(header.Name, value);
        }
    }
}