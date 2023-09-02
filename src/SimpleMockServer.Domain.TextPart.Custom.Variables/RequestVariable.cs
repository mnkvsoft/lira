namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public record RequestVariable : Variable
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    private static readonly object NullValue = new();
    
    public RequestVariable(string name, IReadOnlyCollection<IObjectTextPart> parts) : base(name)
    {
        _parts = parts;
    }

    public override object? Get(RequestData request)
    {
        var key = "variable_" + Name;
        if (request.Items.TryGetValue(key, out var value))
        {
            if (value == NullValue)
                return null;
                
            return value;
        }

        object? newValue = _parts.Generate(request);
        request.Items.Add(key, newValue ?? NullValue);
        return newValue;
    }
}
