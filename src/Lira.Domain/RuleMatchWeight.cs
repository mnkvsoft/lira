namespace Lira.Domain;

class RuleMatchWeight : IComparable<RuleMatchWeight>
{
    private readonly int _weight;
    private readonly IReadOnlyCollection<Matched> _matchedResults;

    public RuleMatchWeight(IReadOnlyCollection<Matched> matchedResults)
    {
        _weight = matchedResults.Sum(x => x.Weight);
        _matchedResults = matchedResults;
    }

    public int CompareTo(RuleMatchWeight? other)
    {
        if (other == null)
            return 1;

        return _weight.CompareTo(other._weight);
    }

    public override string ToString()
    {
        return _weight + " (" + string.Join(", ", _matchedResults.Select(x => x.Name + ": " + x.Weight)) + ")";
    }
}
