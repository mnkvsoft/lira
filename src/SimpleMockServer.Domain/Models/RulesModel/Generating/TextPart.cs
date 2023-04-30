namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public abstract record TextPart
{
    public abstract string? Get(RequestData request);

    public record Static(string Value) : TextPart
    {
        public override string? Get(RequestData request) => Value;
    }

    public record Function(IGeneratingFunction Func) : TextPart
    {
        public override string? Get(RequestData request) => Func.Generate(request);
    }

    public record Variable(string Name, IReadOnlyCollection<TextPart> parts) : TextPart
    {
        public override string? Get(RequestData request)
        {
            var key = "variable_" + Name;
            if (request.Items.TryGetValue(key, out var value))
                return (string)value;

            string? newValue = parts.Generate(request);
            request.Items.Add(key, newValue);
            return newValue;
        }
    }
}
