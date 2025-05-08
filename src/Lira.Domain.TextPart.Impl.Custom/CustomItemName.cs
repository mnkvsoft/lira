using ArgValidation;
// ReSharper disable VirtualMemberCallInConstructor

namespace Lira.Domain.TextPart.Impl.Custom;

public static class CustomItemName
{
    public static bool IsValidName(string prefix, string value)
    {
        Arg.NotNullOrEmpty(prefix, nameof(prefix));

        return !string.IsNullOrWhiteSpace(value) &&
               value.StartsWith(prefix) &&
               IsValidName(value[prefix.Length..]);
    }

    static bool IsValidName(string withoutPrefix)
    {
        if (!IsAllowedFirstCharInName(withoutPrefix[0]))
            return false;

        foreach (char c in withoutPrefix[1..])
        {
            if (IsAllowedCharInName(c))
                continue;
            return false;
        }

        return true;
    }

    public static bool IsAllowedCharInName(char c) => char.IsLetter(c) || char.IsDigit(c) || c == '_' || c == '.';
    public static bool IsAllowedFirstCharInName(char c) => char.IsLetter(c) || char.IsDigit(c) || c == '_';
}