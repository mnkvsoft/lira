namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers;

public interface IComparableMatchFunction<TComparable> where TComparable : IComparable<TComparable>
{
    bool IsMatch(TComparable value);
}
