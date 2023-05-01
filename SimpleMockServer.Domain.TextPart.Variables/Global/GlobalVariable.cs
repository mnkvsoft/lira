using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart.Variables.Global;

public class GlobalVariable : Variable
{
    private readonly string _value;

    public GlobalVariable(string name, IReadOnlyCollection<IGlobalTextPart> parts) : base(name)
    {
        _value = string.Concat(parts.Select(p => p.Get()));
    }

    public override string? Get(RequestData request)
    {
        return _value;
    }
}
