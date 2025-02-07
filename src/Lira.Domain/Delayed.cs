using Lira.Common.PrettyParsers;
using Lira.Domain.Generating;

namespace Lira.Domain;

public record Delayed<T>(T Value, DelayGenerator? DelayGenerator);

public record DelayGenerator(TextParts Parts)
{
    public async Task<TimeSpan> Generate(RuleExecutingContext context)
    {
        string delayStr = await Parts.Generate(context);
        var delay = PrettyTimespanParser.Parse(delayStr);
        return delay;
    }
}