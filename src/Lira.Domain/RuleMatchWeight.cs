using Lira.Common.Exceptions;

namespace Lira.Domain;

public interface IRuleMatchWeight : IComparable<IRuleMatchWeight>
{
}

class RuleMatchWeight : IRuleMatchWeight
{
    private readonly int _weight;
    private readonly IReadOnlyCollection<Matched> _matchedResults;

    public RuleMatchWeight(IReadOnlyCollection<Matched> matchedResults)
    {
        _weight = matchedResults.Sum(x => x.Weight);
        _matchedResults = matchedResults;
    }

    public int CompareTo(IRuleMatchWeight? other)
    {
        if (other == null)
            return 1;
        
        if (other is not RuleMatchWeight otherWeight)
            throw new UnsupportedInstanceType(other);

        return _weight.CompareTo(otherWeight._weight);
    }

    public override string ToString()
    {
        return _weight + " (" + string.Join(", ", _matchedResults.Select(x => x.Name + ": " + x.Weight)) + ")";
    }
}
