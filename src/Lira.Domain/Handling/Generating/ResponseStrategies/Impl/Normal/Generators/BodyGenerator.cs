namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;

public class BodyGenerator(TextPartsProvider partsProvider)
{
    internal IEnumerable<string> Create(RuleExecutingContext context) => partsProvider.Get(context);
}