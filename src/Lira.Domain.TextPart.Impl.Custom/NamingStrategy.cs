using ArgValidation;
// ReSharper disable VirtualMemberCallInConstructor

namespace Lira.Domain.TextPart.Impl.Custom;

public record NamingStrategy(string Prefix, Predicate<char> IsAllowedFirstChar, Predicate<char> IsAllowedChar)
{
    public bool IsValidName(string value)
    {
        Arg.NotNullOrEmpty(Prefix, nameof(Prefix));

        return !string.IsNullOrWhiteSpace(value) &&
               value.StartsWith(Prefix) &&
               IsValidNameAfterPrefix(value[Prefix.Length..]);
    }

    bool IsValidNameAfterPrefix(string withoutPrefix)
    {
        if (!IsAllowedFirstChar(withoutPrefix[0]))
            return false;

        foreach (char c in withoutPrefix[1..])
        {
            if (IsAllowedChar(c))
                continue;
            return false;
        }

        return true;
    }
}