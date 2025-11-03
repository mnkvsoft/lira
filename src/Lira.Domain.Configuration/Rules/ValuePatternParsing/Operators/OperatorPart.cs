using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

abstract class OperatorPart : IObjectTextPart
{
    public abstract dynamic Get(RuleExecutingContext context);
    public Type Type => DotNetType.EnumerableDynamic;
}