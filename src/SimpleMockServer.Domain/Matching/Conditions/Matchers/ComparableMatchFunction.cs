namespace SimpleMockServer.Domain.Matching.Conditions.Matchers;

public class ComparableMatchFunction<TComparable> : IComparableMatchFunction<TComparable> where TComparable : IComparable<TComparable>
{
    private readonly ValueComparer<TComparable> _comparer;

    public ComparableMatchFunction(ValueComparer<TComparable> comparer)
    {
        _comparer = comparer;
    }

    public bool IsMatch(TComparable value)
    {
        return _comparer.IsSatisfy(value);
    }
}
