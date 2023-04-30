using SimpleMockServer.Domain.Models.RulesModel.Generating;

namespace SimpleMockServer.ConfigurationProviding.Rules;

public class VariableSet
{
    private readonly List<TextPart.Variable> _variables = new List<TextPart.Variable>();

    public void Add(TextPart.Variable variable)
    {
        var exist = _variables.FirstOrDefault(x => x.Name == variable.Name) != null;
        if (exist)
            throw new InvalidOperationException($"Variable '{variable.Name}' already declared");
        _variables.Add(variable);   
    }

    public TextPart.Variable GetOrThrow(string name)
    {
        var result = _variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Variable '{name}' not declared");
        return result;
    }
}
