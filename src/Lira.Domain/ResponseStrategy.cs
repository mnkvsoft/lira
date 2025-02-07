using Lira.Domain.Generating;
using Lira.Domain.Generating.Writers;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public abstract record ResponseStrategy(GetDelay? GetDelay)
{
    public async Task Execute(HttpContextData httpContextData)
    {
        if (GetDelay != null)
            await Task.Delay(await GetDelay(httpContextData.RuleExecutingContext));

        await ExecuteInternal(httpContextData);
    }

    protected abstract Task ExecuteInternal(HttpContextData httpContextData);

    public record Normal(GetDelay? GetDelay, IHttCodeGenerator CodeGenerator, BodyGenerator? BodyGenerator, HeadersGenerator? HeadersGenerator) : ResponseStrategy(GetDelay)
    {
        protected override async Task ExecuteInternal(HttpContextData httpContextData)
        {
            var context = httpContextData.RuleExecutingContext;
            var response = httpContextData.Response;

            response.StatusCode = await CodeGenerator.Generate(httpContextData.RuleExecutingContext);

            if (HeadersGenerator != null)
            {
                foreach (var header in await HeadersGenerator.Create(context))
                {
                    response.Headers.Add(header.Name, header.Value);
                }
            }

            if (BodyGenerator != null)
            {
                foreach (string bodyPart in await BodyGenerator.Create(context))
                {
                    await response.WriteAsync(bodyPart);
                }
            }
        }
    }

    public record Abort(GetDelay? GetDelay) : ResponseStrategy(GetDelay)
    {
        protected override Task ExecuteInternal(HttpContextData httpContextData)
        {
            httpContextData.Response.HttpContext.Abort();
            return Task.CompletedTask;
        }
    }
}