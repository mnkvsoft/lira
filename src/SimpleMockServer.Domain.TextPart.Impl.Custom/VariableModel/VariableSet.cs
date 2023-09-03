using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Impl.Custom.VariableModel;

public class VariableSet : UniqueSet<Variable>
{
    public VariableSet(IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public VariableSet()
    {
    }
}