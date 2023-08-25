using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Generating.Writers;

public class BodyWriter
{
    private readonly TextParts _parts;

    public BodyWriter(TextParts parts)
    {
        _parts = parts;
    }

    public async Task Write(HttpContextData httpContextData)
    {
        foreach (var part in _parts)
        {
            string? text = part.Get(httpContextData.Request);
            
            if (text != null)
                await httpContextData.Response.WriteAsync(text);
        }
    }
}