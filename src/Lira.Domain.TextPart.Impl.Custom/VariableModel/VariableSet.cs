using Lira.Common;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class VariableSet : UniqueSet<Variable>
{
    public VariableSet(IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public VariableSet()
    {
    }

    public void TryAddRuntimeVariables(IEnumerable<RuntimeVariable> runtimeVariables)
    {
        foreach (var variable in runtimeVariables)
        {
            TryAdd(variable);
        }
    }
}