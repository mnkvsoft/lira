using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

record StaticPart(string Value) : IObjectTextPart
{
    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(Value);
    public ReturnType ReturnType => ReturnType.String;
}