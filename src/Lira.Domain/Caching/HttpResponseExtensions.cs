using Microsoft.AspNetCore.Http;

namespace Lira.Domain.Caching;

internal static class HttpResponseExtensions
{
    public static async Task Write(this HttpResponse response, ResponseData data)
    {
        response.StatusCode = data.Code;

        if (data.Headers != null)
        {
            foreach (var header in data.Headers)
            {
                response.Headers.Add(header.Name, header.Value);
            }
        }

        if (data.BodyParts != null)
        {
            foreach (string bodyPart in data.BodyParts)
            {
                await response.WriteAsync(bodyPart);
            }
        }
    }
}