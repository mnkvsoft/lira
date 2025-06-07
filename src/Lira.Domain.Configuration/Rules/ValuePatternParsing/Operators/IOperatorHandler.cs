using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

interface IOperatorHandler
{
    string OperatorName { get; }

    IObjectTextPart CreateOperatorPart(OperatorDraft draft);
}