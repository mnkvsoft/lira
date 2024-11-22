using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class VariableSet : UniqueSet<DeclaredVariable>
{
    public VariableSet(IReadOnlyCollection<DeclaredVariable> variables) : base(variables)
    {
    }

    public VariableSet()
    {
    }
}