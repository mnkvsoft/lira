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
            var context = httpContextData.RuleExecutingContext;
            var response = httpContextData.Response;

            response.StatusCode = Code;

            if (HeadersGenerator != null)
            {
                foreach (var header in HeadersGenerator.Create(context))
                {
                    response.Headers.Add(header.Name, header.Value);
                }
            }

            if (BodyGenerator != null)
            {
                foreach (string bodyPart in BodyGenerator.Create(context))
                {
                    await response.WriteAsync(bodyPart);
                }
            }
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