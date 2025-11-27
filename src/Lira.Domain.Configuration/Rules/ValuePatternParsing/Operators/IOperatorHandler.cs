using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

interface IOperatorHandler
{
    OperatorDefinition Definition { get; }

    Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory);
}