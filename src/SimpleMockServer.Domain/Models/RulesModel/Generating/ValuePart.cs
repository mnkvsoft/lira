using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public abstract record ValuePart
{
    public abstract string? Get(HttpRequest request);

    public record Static(string Value) : ValuePart
    {
        public override string? Get(HttpRequest request) => Value;
    }

    public record Dynamic(IGeneratingFunction Function) : ValuePart
    {
        public override string? Get(HttpRequest request) => Function.Generate(request);
    }
}
