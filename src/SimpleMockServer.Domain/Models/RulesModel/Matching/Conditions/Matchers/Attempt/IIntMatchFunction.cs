

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;

//public interface IIntMatchFunction 
//{
//    bool IsMatch(int value);
//}

public interface IComparableMatchFunction<TComparable> where TComparable : IComparable<TComparable>
{
    bool IsMatch(TComparable value);
}

public class Comparer<T> where T : IComparable<T>
{
    private readonly System.Collections.Generic.Comparer<T> _systemComparer = System.Collections.Generic.Comparer<T>.Default;
    private readonly T _value;
    private readonly Predicate<int> _predicate;

    private Comparer(T value, Predicate<int> predicate)
    {
        _value = value;
        _predicate = predicate;
    }

    public bool IsSatisfy(T value)
    {
        return _predicate(_systemComparer.Compare(_value, value));
    }

    public static Comparer<T> AreEquals(T value)
    {
        return new Comparer<T>(value, compareResult => compareResult == 0);
    }

    public static Comparer<T> Less(T value)
    {
        return new Comparer<T>(value, compareResult => compareResult > 0);
    }

    public static Comparer<T> LessOrEquals(T value)
    {
        return new Comparer<T>(value, compareResult => compareResult > 0 || compareResult == 0);
    }

    public static Comparer<T> More(T value)
    {
        return new Comparer<T>(value, compareResult => compareResult < 0);
    }

    public static Comparer<T> MoreOrEquals(T value)
    {
        return new Comparer<T>(value, compareResult => compareResult < 0 || compareResult == 0);
    }
}

public class ComparableMatchFunction<TComparable> : IComparableMatchFunction<TComparable> where TComparable : IComparable<TComparable>
{
    private readonly Comparer<TComparable> _comparer;

    public ComparableMatchFunction(Comparer<TComparable> comparer)
    {
        _comparer = comparer;
    }

    public bool IsMatch(TComparable value)
    {
        return _comparer.IsSatisfy(value);
    }
}


