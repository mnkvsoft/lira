using System.Collections;

namespace SimpleMockServer.Domain.TextPart.Variables;

public class VariableSet : IReadOnlyCollection<Variable>
{
    private readonly HashSet<Variable> _variables = new();

    public VariableSet()
    {
    }

    public VariableSet(IReadOnlyCollection<Variable> set)
    {
        AddRange(set);
    }

    public void Add(Variable variable)
    {
        if (_variables.TryGetValue(variable, out _))
            throw new InvalidOperationException($"Variable '{variable.Name}' already declared");
        _variables.Add(variable);
    }

    public void AddRange(IReadOnlyCollection<Variable> set)
    {
        foreach(var variable in set)
        {
            Add(variable);
        }
    }

    public int Count => _variables.Count;

    public IEnumerator<Variable> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
