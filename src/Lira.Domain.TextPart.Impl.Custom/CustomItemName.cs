using ArgValidation;

namespace Lira.Domain.TextPart.Impl.Custom;

public readonly struct CustomItemName : IEquatable<CustomItemName>
{
    public string Value { get; }

    public CustomItemName(string value)
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

    public bool Equals(CustomItemName other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is CustomItemName other && Equals(other);
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