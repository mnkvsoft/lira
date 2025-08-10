using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

interface IOperatorHandler
{
    OperatorDefinition Definition { get; }

    Task<IObjectTextPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory);
}