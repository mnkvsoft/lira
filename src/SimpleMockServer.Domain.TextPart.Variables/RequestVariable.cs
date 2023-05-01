using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart.Variables;

public class RequestVariable : Variable
{
    private readonly IReadOnlyCollection<ITextPart> _parts;

    public RequestVariable(string name, IReadOnlyCollection<ITextPart> parts) : base(name)
    {
        _parts = parts;
    }

    public override string? Get(RequestData request)
    {
        var key = "variable_" + Name;
        if (request.Items.TryGetValue(key, out var value))
            return (string)value;

        var newValue = _parts.Generate(request);
        request.Items.Add(key, newValue);
        return newValue;
    }
}
