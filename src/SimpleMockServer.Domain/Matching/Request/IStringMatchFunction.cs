namespace SimpleMockServer.Domain.Matching.Request;

public enum MatchFunctionRestriction
{
    Any,
    Type,
    Range
}

public interface IStringMatchFunction
{
    MatchFunctionRestriction Restriction { get; }
    
    bool IsMatch(string? value);
}
