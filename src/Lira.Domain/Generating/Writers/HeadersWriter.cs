using ArgValidation;

namespace Lira.Domain.Generating.Writers;

public class HeadersGenerator
{
    private readonly IReadOnlyCollection<GeneratingHeader> _headers;

    public HeadersGenerator(IReadOnlyCollection<GeneratingHeader> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    internal IReadOnlyCollection<Header> Create(RequestData request)
    {
        var result = new List<Header>();
        foreach (var header in _headers)
        {
            var value = header.TextParts.Generate(request);
            result.Add(new Header(header.Name, value));
        }

        return result;
    }
}