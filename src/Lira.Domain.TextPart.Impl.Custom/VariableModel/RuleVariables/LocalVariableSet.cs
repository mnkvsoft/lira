using Lira.Common;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

public class LocalVariableSet : UniqueSet<LocalVariable>
{
    public void TryAddLocalVariables(IEnumerable<LocalVariable> runtimeVariables)
    {
        foreach (var variable in runtimeVariables)
        {
            TryAdd(variable);
        }
    }
}