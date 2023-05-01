namespace SimpleMockServer.Domain.TextPart.Variables;

public class GlobalVariable : Variable, IGlobalTextPart
{
    private readonly string _value;

    public GlobalVariable(string name, IReadOnlyCollection<IGlobalTextPart> parts) : base(name)
    {
        _value = string.Concat(parts.Select(p => p.Get()));
    }

    public override string? Get(RequestData request) => _value;

    public string? Get() => _value;
}
