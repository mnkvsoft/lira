namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public record GlobalVariable : Variable, IGlobalObjectTextPart
{
    private readonly object? _value;

    public GlobalVariable(string name, IReadOnlyCollection<IGlobalObjectTextPart> parts) : base(name)
    {
        _value = parts.Generate();
    }

    public override object? Get(RequestData request) => _value;

    public object? Get() => _value;
}
