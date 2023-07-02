namespace SimpleMockServer.Domain.Matching.Conditions.Matchers;

public class ValueComparer<T> where T : IComparable<T>
{
    private readonly Comparer<T> _systemComparer = Comparer<T>.Default;
    private readonly T _value;
    private readonly Predicate<int> _predicate;

    private ValueComparer(T value, Predicate<int> predicate)
    {
        _value = value;
        _predicate = predicate;
    }

    public bool IsSatisfy(T value)
    {
        return _predicate(_systemComparer.Compare(_value, value));
    }

    public static ValueComparer<T> AreEquals(T value)
    {
        return new ValueComparer<T>(value, compareResult => compareResult == 0);
    }

    public static ValueComparer<T> Less(T value)
    {
        return new ValueComparer<T>(value, compareResult => compareResult > 0);
    }

    public static ValueComparer<T> LessOrEquals(T value)
    {
        return new ValueComparer<T>(value, compareResult => compareResult is > 0 or 0);
    }

    public static ValueComparer<T> More(T value)
    {
        return new ValueComparer<T>(value, compareResult => compareResult < 0);
    }

    public static ValueComparer<T> MoreOrEquals(T value)
    {
        return new ValueComparer<T>(value, compareResult => compareResult is < 0 or 0);
    }
}
