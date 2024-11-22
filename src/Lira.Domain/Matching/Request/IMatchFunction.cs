namespace Lira.Domain.Matching.Request;

public interface IMatchFunction
{
    MatchFunctionRestriction Restriction { get; }
    Task<bool> IsMatch(RuleExecutingContext context, string? value);
}