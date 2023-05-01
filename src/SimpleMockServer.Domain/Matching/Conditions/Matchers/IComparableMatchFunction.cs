namespace SimpleMockServer.Domain.Matching.Conditions.Matchers;

public interface IComparableMatchFunction<TComparable> where TComparable : IComparable<TComparable>
{
    bool IsMatch(TComparable value);
}
