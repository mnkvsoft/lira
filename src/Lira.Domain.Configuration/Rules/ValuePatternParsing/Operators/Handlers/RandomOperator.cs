using System.Collections.Immutable;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RandomHandler : IOperatorHandler
{
    public string OperatorName => OperatorPart.Prefix + "random";
    public IObjectTextPart CreateOperatorPart(OperatorDraft draft) => new RandomOperator(draft);
}

class RandomOperator : IObjectTextPart
{
    private readonly IReadOnlyList<IReadOnlyCollection<IObjectTextPart>> _items;

    public RandomOperator(OperatorDraft draft)
    {
        string item = OperatorPart.Prefix + "item";
        var operatorData = OperatorParser.Parse(draft, item);
        if(operatorData.Body.Count > 0)
            throw new Exception("@random declaration must contain at least one @item.");
        _items = operatorData.Items.Select(x => x.Body).ToImmutableArray();
    }

    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var randomItems = _items.Random();
        foreach (var part in randomItems)
        {
            yield return part.Get(context);
        }
    }

    public ReturnType ReturnType => ReturnType.String;
}