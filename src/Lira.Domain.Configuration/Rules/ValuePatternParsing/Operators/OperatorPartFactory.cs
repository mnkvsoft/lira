using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

class OperatorPartFactory(IEnumerable<IOperatorHandler> handlers)
{
    private List<string>? _registeredOperators;
    public IReadOnlyCollection<string> RegisteredOperators => _registeredOperators ??= handlers.Select(x => x.OperatorName).ToList();

    public IObjectTextPart CreateOperatorPart(OperatorDraft draft)
    {
        foreach (var handler in handlers)
        {
            if(handler.OperatorName == draft.Name)
                return handler.CreateOperatorPart(draft);
        }
        throw new Exception($"No operator handler found for {draft.Name}");
    }
}