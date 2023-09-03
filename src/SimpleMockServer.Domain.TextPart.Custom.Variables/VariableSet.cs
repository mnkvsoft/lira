using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public class VariableSet : UniqueSet<Variable>
{
    public VariableSet(IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public VariableSet()
    {
    }
}