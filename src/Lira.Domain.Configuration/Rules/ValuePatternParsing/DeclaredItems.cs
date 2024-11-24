using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IReadonlyDeclaredItems
{
    IReadOnlyCollection<Variable> Variables { get; }

    IReadOnlyCollection<Function> Functions { get; }
}
class DeclaredItems : IReadonlyDeclaredItems
{
    public VariableSet Variables { get; }
    IReadOnlyCollection<Variable> IReadonlyDeclaredItems.Variables => Variables;

    public FunctionSet Functions { get; }
    IReadOnlyCollection<Function> IReadonlyDeclaredItems.Functions => Functions;

    public DeclaredItems(IReadonlyDeclaredItems items)
    {
        Variables = new VariableSet(items.Variables);
        Functions = new FunctionSet(items.Functions);
    }

    public DeclaredItems()
    {
        Variables = new VariableSet();
        Functions = new FunctionSet();
    }

    public void Add(IReadonlyDeclaredItems items)
    {
        Variables.AddRange(items.Variables);
        Functions.AddRange(items.Functions);
    }
}