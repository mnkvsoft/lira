using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating.Writers;

public class BodyWriter
{
    private readonly ValuePartSet _parts;

    public BodyWriter(ValuePartSet parts)
    {
        _parts = parts;
    }

    public async Task Write(HttpContextData httpContextData)
    {
        foreach (var part in _parts)
        {
            string? value = part.Get(httpContextData.Request);
            await httpContextData.Response.WriteAsync(value);
        }
    }
}