using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider(ISet<DeclaredItem> items) : IDeclaredPartsProvider
{
    public IObjectTextPart Get(string name)
    {

        return items.SingleOrDefault(x => x.Name == name) ?? throw new Exception($"Unknown declaration '{name}'");
    }

    public void SetVariable(string name, RuleExecutingContext context, dynamic value)
    {
        var variable = items.OfType<Variable>().Single(v => v.Name == name);
        variable.SetValue(context, value);
    }
}