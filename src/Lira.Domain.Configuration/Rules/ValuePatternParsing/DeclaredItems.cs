using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredItems : HashSet<DeclaredItem>
{
    private DeclaredItems(IReadOnlySet<DeclaredItem> items) : base(items.Where(x => x is not LocalVariable))
    {
    }

    public DeclaredItems()
    {
    }

    public static DeclaredItems WithoutLocalVariables(IReadOnlySet<DeclaredItem> items) => new(items);

    public override string ToString()
    {
        var nl = Environment.NewLine;
        return
            $"- declared functions: {string.Join(", ", this.OfType<Function>().Select(x => x.Name))}" +
            nl +
            $"- declared variables: {string.Join(", ", this.OfType<RuleVariable>().Select(x => x.Name))}"+
            nl +
            $"- declared local variables: {string.Join(", ", this.OfType<LocalVariable>().Select(x => x.Name))}"
            ;
    }
}