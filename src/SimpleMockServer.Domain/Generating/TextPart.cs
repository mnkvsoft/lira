namespace SimpleMockServer.Domain.Generating;

public interface ITextPart
{
    string? Get(RequestData request);
}

public record Static(string Value) : ITextPart
{
    public string? Get(RequestData request) => Value;
}

public record RequestVariable(string Name, IReadOnlyCollection<ITextPart> parts) : ITextPart
{
    public string? Get(RequestData request)
    {
        var key = "variable_" + Name;
        if (request.Items.TryGetValue(key, out var value))
            return (string)value;

        var newValue = parts.Generate(request);
        request.Items.Add(key, newValue);
        return newValue;
    }
}
