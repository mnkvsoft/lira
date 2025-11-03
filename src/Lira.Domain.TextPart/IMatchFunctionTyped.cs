using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart;

public interface IMatchFunctionTyped : IMatchFunction
{
    Type ValueType { get; }
    bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue);
}