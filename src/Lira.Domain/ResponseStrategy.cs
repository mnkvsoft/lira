using Lira.Domain.Generating.Writers;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public abstract record ResponseStrategy(TimeSpan? Delay)
{
    public async Task Execute(HttpContextData httpContextData)
    {
        if (Delay != null)
            await Task.Delay(Delay.Value);

        await ExecuteInternal(httpContextData);
    }

    protected abstract Task ExecuteInternal(HttpContextData httpContextData);
    
    public record Normal(TimeSpan? Delay, int Code, BodyGenerator? BodyGenerator, HeadersGenerator? HeadersGenerator) : ResponseStrategy(Delay)
    {
        protected override async Task ExecuteInternal(HttpContextData httpContextData)
        {
            var request = httpContextData.Request;
            var response = httpContextData.Response;
            
            if (HeadersGenerator != null)
            {
                foreach (var header in HeadersGenerator.Create(request))
                {
                    response.Headers.Add(header.Name, header.Value);
                }
            }

            if (BodyGenerator != null)
            {
                foreach (string bodyPart in BodyGenerator.Create(request))
                {
                    await response.WriteAsync(bodyPart);
                }
            }

            response.StatusCode = Code;
        }
    }

    public record Abort(TimeSpan? Delay) : ResponseStrategy(Delay)
    {
        protected override Task ExecuteInternal(HttpContextData httpContextData)
        {
            httpContextData.Response.HttpContext.Abort();
            return Task.CompletedTask;
        }
    }
}