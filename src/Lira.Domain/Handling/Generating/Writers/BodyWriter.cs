namespace Lira.Domain.Handling.Generating.Writers;

// todo: remove?
public class BodyGenerator(TextPartsProvider partsProvider)
{
    internal IEnumerable<string> Create(RuleExecutingContext context) => partsProvider.Get(context);
}