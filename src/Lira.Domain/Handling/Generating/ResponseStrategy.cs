using Lira.Domain.Handling.Generating.Writers;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain.Handling.Generating;

public abstract record ResponseGenerationHandler : IHandler
{
    public abstract Task Handle(HttpContextData httpContextData);

    public record Normal(IHttCodeGenerator CodeGenerator, BodyGenerator? BodyGenerator, HeadersGenerator? HeadersGenerator) : ResponseGenerationHandler
    {
        public override async Task Handle(HttpContextData httpContextData)
        {
            var context = httpContextData.RuleExecutingContext;
            var response = httpContextData.Response;

            response.StatusCode = CodeGenerator.Generate(httpContextData.RuleExecutingContext);

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

    public record Abort : ResponseGenerationHandler
    {
        public override Task Handle(HttpContextData httpContextData)
        {
            httpContextData.Response.HttpContext.Abort();
            return Task.CompletedTask;
        }
    }
}