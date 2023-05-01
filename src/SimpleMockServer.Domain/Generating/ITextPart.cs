namespace SimpleMockServer.Domain.Generating;

public interface ITextPart
{
    string? Get(RequestData request);
}

public interface IGlobalTextPart : ITextPart
{
    string? Get();
}

public record Static(string Value) : IGlobalTextPart
{
    public string? Get(RequestData request) => Value;

    public string? Get() => Value;
}
