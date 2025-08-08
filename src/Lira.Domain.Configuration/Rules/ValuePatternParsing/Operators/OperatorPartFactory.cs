using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

class OperatorPartFactory(IEnumerable<IOperatorHandler> handlers)
{
    public Task<IObjectTextPart> CreateOperatorPart(Token.Operator @operator, IParsingContext localContext)
    {
        var name = @operator.Definition.Name;
        foreach (var handler in handlers)
        {
            if(handler.Definition.Name == name)
                return handler.CreateOperatorPart(@operator, localContext, this);
        }
        throw new Exception($"No operator handler found for {name}");
    }
}