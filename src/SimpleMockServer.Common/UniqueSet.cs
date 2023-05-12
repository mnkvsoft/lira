using System.Collections;

namespace SimpleMockServer.Common;

public interface IUniqueSetItem
{
    string Name { get; }
    string EntityName { get; }
}
    
public abstract class UniqueSet<T> : IReadOnlyCollection<T> where T : IUniqueSetItem
{
    private readonly HashSet<T> _variables = new();

    public UniqueSet()
    {
    }

    public UniqueSet(IReadOnlyCollection<T> set)
    {
        AddRange(set);
    }

    public void Add(T item)
    {
        if (_variables.TryGetValue(item, out _))
            throw new InvalidOperationException($"{item.EntityName} '{item.Name}' already declared");
        _variables.Add(item);
    }

    public void AddRange(IReadOnlyCollection<T> items)
    {
        foreach(var item in items)
        {
            Add(item);
        }
    }

    public int Count => _variables.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
