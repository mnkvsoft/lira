using ArgValidation;

namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public readonly struct DeclaredItemName : IEquatable<DeclaredItemName>
{
    public string Value { get; }

    public DeclaredItemName(string value)
    {
        Arg.Validate(value, nameof(value))
            .NotNullOrWhitespace();
        
        if (!IsValidName(value))
            throw new ArgumentException($"Variable name must contains only letters or '_','.'. Current: '{value}'");

        Value = value;
    }

    public static bool IsValidName(string value)
    {
        foreach (char c in value)
        {
            if (IsAllowedCharInName(c))
                continue;
            return false;
        }

        return true;
    }

    public static bool IsAllowedCharInName(char c) => char.IsLetter(c) || c == '_' || c == '.';

    public bool Equals(DeclaredItemName other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is DeclaredItemName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}