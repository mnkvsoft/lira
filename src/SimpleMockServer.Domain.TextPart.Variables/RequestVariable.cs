namespace SimpleMockServer.Domain.TextPart.Variables;

public class RequestVariable : Variable
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;

    public RequestVariable(string name, IReadOnlyCollection<IObjectTextPart> parts) : base(name)
    {
        _parts = parts;
    }

    public override object Get(RequestData request)
    {
        var key = "variable_" + Name;
        if (request.Items.TryGetValue(key, out var value))
            return value;

        object? newValue = _parts.Generate(request);
        request.Items.Add(key, newValue);
        return newValue;
    }
}
