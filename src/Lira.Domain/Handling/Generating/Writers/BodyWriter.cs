namespace Lira.Domain.Handling.Generating.Writers;

// todo: remove?
public class BodyGenerator(TextPartsProvider partsProvider)
{
    internal IAsyncEnumerable<string> Create(RuleExecutingContext context) => partsProvider.Get(context);
}