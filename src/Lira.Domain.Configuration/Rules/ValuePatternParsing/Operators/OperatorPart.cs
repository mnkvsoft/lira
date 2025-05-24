using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

abstract class OperatorPart : IObjectTextPart
{
    public abstract IEnumerable<dynamic?> Get(RuleExecutingContext context);
    public ReturnType ReturnType => ReturnType.String;
}