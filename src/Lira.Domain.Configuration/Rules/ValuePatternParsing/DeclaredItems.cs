using System.Collections;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IReadonlyDeclaredItems : IEnumerable<DeclaredItem>
{
    IReadOnlyCollection<RuleVariable> Variables { get; }
    IReadOnlyCollection<LocalVariable> LocalVariables { get; }
    IReadOnlyCollection<Function> Functions { get; }
}
class DeclaredItems : IReadonlyDeclaredItems
{
    public VariableSet Variables { get; }
    IReadOnlyCollection<RuleVariable> IReadonlyDeclaredItems.Variables => Variables;

    public LocalVariableSet LocalVariables { get; }
    IReadOnlyCollection<LocalVariable> IReadonlyDeclaredItems.LocalVariables => LocalVariables;

    public FunctionSet Functions { get; }
    IReadOnlyCollection<Function> IReadonlyDeclaredItems.Functions => Functions;


    public DeclaredItems(IReadonlyDeclaredItems items)
    {
        Variables = new VariableSet(items.Variables);
        Functions = new FunctionSet(items.Functions);
        LocalVariables = new LocalVariableSet();
    }

    public DeclaredItems()
    {
        Variables = new VariableSet();
        Functions = new FunctionSet();
        LocalVariables = new LocalVariableSet();
    }

    public void Add(IReadonlyDeclaredItems items)
    {
        Variables.AddRange(items.Variables);
        Functions.AddRange(items.Functions);
    }

    public IEnumerator<DeclaredItem> GetEnumerator()
    {
        return Variables.Cast<DeclaredItem>().Union(Functions).Union(LocalVariables).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var nl = Environment.NewLine;
        return
            $"Declared functions: {string.Join(", ", Functions.Select(x => Consts.ControlChars.FunctionPrefix + x.Name))}" +
            nl +
            $"Declared variables: {string.Join(", ", Variables.Select(x => Consts.ControlChars.RuleVariablePrefix + x.Name))}"+
            nl +
            $"Declared local variables: {string.Join(", ", LocalVariables.Select(x => Consts.ControlChars.LocalVariablePrefix + x.Name))}"
            ;
    }
}