namespace Lira.Common.Extensions;

public static class ComparableExtensions
{
    public static bool AreEquals<T>(this T value, T other) where T : IComparable<T>
    {
        return value.CompareTo(other) == 0;
    }

    public static bool LessThan<T>(this T value, T other) where T : IComparable<T>
    {
        return value.CompareTo(other) < 0;
    }

    public static bool LessOrEqualsThan<T>(this T value, T other) where T : IComparable<T>
    {
        int compareResult = value.CompareTo(other);
        return compareResult is < 0 or 0;
    }

    public static bool MoreThan<T>(this T value, T other) where T : IComparable<T>
    {
        return value.CompareTo(other) > 0;
    }

    public static bool MoreOrEqualsThan<T>(this T value, T other) where T : IComparable<T>
    {
        int compareResult = value.CompareTo(other);
        return compareResult is > 0 or 0;
    }
}
