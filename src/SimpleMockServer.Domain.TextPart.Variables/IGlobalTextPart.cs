using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart.Variables;

public interface IGlobalTextPart : ITextPart
{
    string? Get();
}
