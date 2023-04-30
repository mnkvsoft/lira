using ArgValidation;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;

public class HeadersWriter
{
    private readonly IReadOnlyCollection<GeneratingHeader> _headers;

    public HeadersWriter(IReadOnlyCollection<GeneratingHeader> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    public void Write(HttpContextData httpContextData)
    {
        var response = httpContextData.Response;

        foreach (var header in _headers)
        {
            string value = header.TextParts.Generate(httpContextData.Request);

            if (header.Name == "Content-Type")
                response.ContentType = value;
            else
                response.Headers.Add(header.Name, value);
        }
    }
}
