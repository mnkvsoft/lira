using System.Collections;

namespace SimpleMockServer.Domain.TextPart.Variables;

public class VariableSet<TVariable> : IReadOnlyCollection<TVariable> where TVariable : Variable
{
    private readonly HashSet<TVariable> _variables = new HashSet<TVariable>();

    public void Add(TVariable variable)
    {
        if (_variables.TryGetValue(variable, out _))
            throw new InvalidOperationException($"Variable '{variable.Name}' already declared");
        _variables.Add(variable);
    }

    public int Count => _variables.Count;

    public IEnumerator<TVariable> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

