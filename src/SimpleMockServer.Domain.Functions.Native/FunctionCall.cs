namespace SimpleMockServer.Domain.Functions.Native;

public record MethodCall(string Name, IReadOnlyList<string> Argumens)
{
    public override string ToString()
    {
        return Name + "(" + string.Join(", ", Argumens) + ")";
    }
}