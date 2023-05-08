namespace SimpleMockServer.Domain.TextPart.Variables;

public class GlobalObjectVariable : Variable, IGlobalObjectTextPart
{
    private readonly object _value;

    public GlobalObjectVariable(string name, IReadOnlyCollection<IGlobalObjectTextPart> parts) : base(name)
    {
        _value = parts.Generate();
    }

    public override object Get(RequestData request) => _value;

    public object Get() => _value;
}
