using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RandomOperatorDefinition() : OperatorDefinition(
    "random",
    ParametersMode.None,
    withBody: true,
    allowedChildElements: new Dictionary<string, ParametersMode> { { "item", ParametersMode.None } });

class RandomHandler(TextPartsParserInternal parser, RandomOperatorDefinition definition) : IOperatorHandler
{
    public OperatorDefinition Definition => definition;

    public async Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        if(@operator.Elements.Count < 2)
            throw new Exception("The @random operator must have at least two 'item' elements");

        var items = new List<IReadOnlyCollection<IObjectTextPart>>();
        foreach (var item in @operator.Elements)
        {
            items.Add(await parser.Parse(item.Content, context, operatorPartFactory));
        }

        return new RandomOperator(items);
    }

    class RandomOperator(IReadOnlyList<IReadOnlyCollection<IObjectTextPart>> items) : OperatorPart
    {
        public override async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            var randomItems = items.Random();
            await foreach (var obj in randomItems.GetAllObjects(context))
            {
                yield return obj;
            }
        }
    }
}