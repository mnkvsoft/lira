using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatHandler(TextPartsParserInternal parser) : IOperatorHandler
{
    public OperatorDefinition Definition { get; } = new("repeat", ParametersMode.Maybe, withBody: true, allowedChildElements: null);
    public async Task<IObjectTextPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        return new RepeatOperator(await parser.Parse(@operator.Content, context, operatorPartFactory));
    }

    class RepeatOperator(IReadOnlyCollection<IObjectTextPart> body) : IObjectTextPart
    {
        public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            int count = Random.Shared.Next(1, 5);

            for (int i = 0; i < count; i++)
            {
                if(i > 0)
                    yield return ',';

                await foreach (var obj in body.GetAllObjects(context))
                {
                    yield return obj;
                }
            }
        }

        public ReturnType ReturnType => ReturnType.String;
    }
}

