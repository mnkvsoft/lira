using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class IfOperatorDefinition() : OperatorDefinition("if",
    ParametersMode.Required,
    withBody: true,
    allowedChildElements: new Dictionary<string, ParametersMode>
    {
        { "else", ParametersMode.Maybe },
        { "else if", ParametersMode.Maybe }
    });

class IfHandler(TextPartsParserInternal parser, IfOperatorDefinition operatorDefinition) : IOperatorHandler
{
    public OperatorDefinition Definition => operatorDefinition;

    public Task<IObjectTextPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context, OperatorPartFactory operatorPartFactory)
    {
        throw new NotImplementedException();
    }

    class IfOperator(IReadOnlyCollection<IObjectTextPart> body) : IObjectTextPart
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

        // todo: string ?
        public ReturnType ReturnType => ReturnType.String;
    }
}

