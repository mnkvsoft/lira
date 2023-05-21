using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
public class DynamicObjectTextPartBase
{
    private readonly IReadOnlyCollection<Variable> _variables;
    private readonly IReadOnlyCollection<GlobalVariable> _globalVariables;

    public DynamicObjectTextPartBase(IReadOnlyCollection<Variable> variables)
    {
        _variables = variables;
        _globalVariables = _variables.Where(v => v is GlobalVariable).Cast<GlobalVariable>().ToList();
    }

    public dynamic? GetVariable(string name, RequestData? request)
    {
        if (request != null)
        {
            return _variables.GetOrThrow(name).Get(request);    
        }
        
        var globalVariable = (GlobalVariable)_globalVariables.GetOrThrow(name);
        return globalVariable.Get();
    }
}
