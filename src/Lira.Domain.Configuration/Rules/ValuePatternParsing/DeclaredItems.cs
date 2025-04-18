﻿using System.Collections;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IReadonlyDeclaredItems : IEnumerable<DeclaredItem>
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

    public IEnumerator<DeclaredItem> GetEnumerator()
    {
        return Variables.Cast<DeclaredItem>().Union(Functions).GetEnumerator();
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
            $"Declared variables: {string.Join(", ", Variables.Select(x => Consts.ControlChars.VariablePrefix + x.Name))}";
    }
}