using System.Collections;

namespace SimpleMockServer.Domain.Configuration.Templating;

public class TemplateSet : IReadOnlyCollection<Template>
{
    private readonly HashSet<Template> _variables = new();

    public TemplateSet()
    {
    }

    public TemplateSet(IReadOnlyCollection<Template> set)
    {
        foreach(var template in set)
        {
            Add(template);
        }
    }

    public void Add(Template template)
    {
        if (_variables.TryGetValue(template, out _))
            throw new InvalidOperationException($"Template '{template.Name}' already declared");
        _variables.Add(template);
    }

    public int Count => _variables.Count;

    public IEnumerator<Template> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
