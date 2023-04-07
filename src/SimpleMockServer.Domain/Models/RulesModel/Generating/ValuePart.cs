using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public abstract record ValuePart
{
    public abstract string? Get(HttpRequest request);

    public record Static(string Value) : ValuePart
    {
        public override string? Get(HttpRequest request) => Value;
    }

    public record Function(IGeneratingFunction Func) : ValuePart
    {
        public override string? Get(HttpRequest request) => Func.Generate(request);
    }

    public record Variable(string Name, IGeneratingFunction Func) : ValuePart
    {
        public override string? Get(HttpRequest request)
        {
            var items = request.HttpContext.Items;

            var key = "variable_" + Name;
            if (items.TryGetValue(key, out var value))
                return (string)value;

            string? newValue = Func.Generate(request);
            items.Add(key, newValue);
            return newValue;
        }
    }
}
