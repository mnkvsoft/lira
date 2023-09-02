using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public class VariableSet : UniqueSet<Variable>
{
    public VariableSet()
    {
    }
    
    public VariableSet(IReadOnlyCollection<Variable> set) : base(set)
    {
    }
}
