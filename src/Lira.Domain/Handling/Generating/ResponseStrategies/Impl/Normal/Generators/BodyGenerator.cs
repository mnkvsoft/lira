namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;

public interface IBodyGenerator
{
    IEnumerable<string> Create(RuleExecutingContext context);
}

public class BodyGenerator(TextPartsProvider partsProvider) : IBodyGenerator
{
    public IEnumerable<string> Create(RuleExecutingContext context) => partsProvider.Get(context);
}