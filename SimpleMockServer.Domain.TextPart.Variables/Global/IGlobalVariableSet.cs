namespace SimpleMockServer.Domain.TextPart.Variables.Global;

public interface IGlobalVariableSet : IReadOnlyCollection<GlobalVariable>
{
    void Add(GlobalVariable variable);
}

public class GlobalVariableSet : VariableSet<GlobalVariable>, IGlobalVariableSet
{ 
}
