using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.TextPart;

public interface IDeclaredItemsReadonlyProvider
{
    IObjectTextPart Get(string name);
     bool TryGet(string name, [MaybeNullWhen(false)]out IObjectTextPart result);
     bool ItsAccessToDeclaredItem(string name, [MaybeNullWhen(false)]out IObjectTextPart part);
}

public interface IDeclaredItemsProvider : IDeclaredItemsReadonlyProvider
{
    void SetVariable(string name, RuleExecutingContext context, dynamic value);
}