using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart;

public interface IMatchFunctionTyped : IMatchFunction
{
    PartType ValueType { get; }
}