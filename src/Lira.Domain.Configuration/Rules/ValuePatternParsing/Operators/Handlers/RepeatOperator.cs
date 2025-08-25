using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatOperatorDefinition()
    : OperatorDefinition("repeat", ParametersMode.Maybe, withBody: true, allowedChildElements: null);

class RepeatHandler(TextPartsParserInternal parser, RepeatOperatorDefinition definition) : IOperatorHandler
{
    public OperatorDefinition Definition => definition;
    public async Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        return new RepeatOperator(await parser.Parse(@operator.Content, context, operatorPartFactory));
    }

    class RepeatOperator(IReadOnlyCollection<IObjectTextPart> body) : OperatorPart
    {
        public override async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            int count = Random.Shared.Next(1, 5);

            for (int i = 0; i < count; i++)
            {
                if(i > 0)
                    // todo: use either the value passed by the user or try to calculate it based on the Content-Type header
                    yield return ",\n";

                await foreach (var obj in body.GetAllObjects(context))
                {
                    yield return obj;
                }
            }
        }
    }
}

