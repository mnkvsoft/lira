using System.Text;
using ArgValidation;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;

public class HeadersWriter
{
    private readonly IDictionary<string, ValuePartSet> _headers;

    public HeadersWriter(IDictionary<string, ValuePartSet> headers)
    {
        Arg.NotEmpty(headers, nameof(headers));
        _headers = headers;
    }

    public void Write(HttpContextData httpContextData)
    {
        var response = httpContextData.Response;

        foreach (var header in _headers)
        {
            StringBuilder sbValue = new StringBuilder();
            var parts = header.Value;
            foreach (var part in parts)
            {
                sbValue.Append(part.Get(httpContextData.Request));
            }

            string value = sbValue.ToString();
            if (header.Key == "Content-Type")
                response.ContentType = value;
            else
                response.Headers.Add(header.Key, value);
        }
    }
}