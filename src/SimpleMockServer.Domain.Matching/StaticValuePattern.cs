namespace SimpleMockServer.Domain.Matching;

public record StaticValuePattern(string Expected) : IValuePattern
{
    public bool IsMatch(string current)
    {
        return Expected.Equals(current, StringComparison.OrdinalIgnoreCase);
    }
}
