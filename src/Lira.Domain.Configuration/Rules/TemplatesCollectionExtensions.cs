using Lira.Domain.Configuration.Templating;

namespace Lira.Domain.Configuration.Rules;

static class TemplatesCollectionExtensions
{
    public static Template GetOrThrow(this IReadOnlyCollection<Template> variables, string name)
    {
        var result = variables.FirstOrDefault(v => v.Name == name);
        if (result == null)
            throw new InvalidOperationException($"Template '{name}' not declared");
        return result;
    }
}