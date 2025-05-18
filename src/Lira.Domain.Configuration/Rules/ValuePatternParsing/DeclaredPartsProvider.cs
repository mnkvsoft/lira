using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider(DeclaredItemsRegistry registry) : IDeclaredPartsProvider
{
    public IObjectTextPart Get(string name) => registry.Get(name);

    public void SetVariable(string name, RuleExecutingContext context, dynamic value) =>
        registry.SetVariable(name, context, value);
}