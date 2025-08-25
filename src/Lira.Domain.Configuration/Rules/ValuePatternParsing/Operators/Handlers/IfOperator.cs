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

    public Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context, OperatorPartFactory operatorPartFactory)
    {
        throw new NotImplementedException();
    }

    class IfOperator(IReadOnlyCollection<IObjectTextPart> body) : OperatorPart
    {
        public override async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            throw new NotImplementedException();
            yield return null;
        }
    }
}

