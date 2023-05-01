using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.Configuration.Rules;

public class VariableSet
{
    private readonly List<RequestVariable> _variables = new List<RequestVariable>();

    public void Add(RequestVariable variable)
    {
        var exist = _variables.FirstOrDefault(x => x.Name == variable.Name) != null;
        if (exist)
            throw new InvalidOperationException($"RequestVariable '{variable.Name}' already declared");
        _variables.Add(variable);
    }

    public RequestVariable GetOrThrow(string name)
    {
        var result = _variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"RequestVariable '{name}' not declared");
        return result;
    }
}
