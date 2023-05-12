using System.Collections;

namespace SimpleMockServer.Domain.Configuration.Templating;

public class TemplateSet : IReadOnlyCollection<Template>
{
    private readonly HashSet<Template> _templates = new();

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

    public void AddRange(IReadOnlyCollection<Template> set)
    {
        foreach(var template in set)
        {
            Add(template);
        }
    }

    public void Add(Template template)
    {
        if (_templates.TryGetValue(template, out _))
            throw new InvalidOperationException($"Template '{template.Name}' already declared");
        _templates.Add(template);
    }

    public int Count => _templates.Count;

    public IEnumerator<Template> GetEnumerator()
    {
        return _templates.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
