namespace Lira.Domain.Matching.Request;

public interface IMatchFunction
{
    MatchFunctionRestriction Restriction { get; }
    
    bool IsMatch(string? value);
}