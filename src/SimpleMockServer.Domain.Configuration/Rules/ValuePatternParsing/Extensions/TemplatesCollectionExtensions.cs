using SimpleMockServer.Domain.Configuration.Templating;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing.Extensions
{
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
}
