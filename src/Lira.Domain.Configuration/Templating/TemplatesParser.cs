using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Templating;

static class TemplatesParser
{
    public static TemplateSet Parse(IReadOnlyCollection<string> lines)
    {
        var result = new TemplateSet();

        foreach (var line in lines)
        {
            var (name, template) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();

            if (string.IsNullOrEmpty(name))
                throw new Exception($"Template name not defined. Line: {line}");

            if (string.IsNullOrEmpty(template))
                throw new Exception($"Template '{name}' not initialized. Line: {line}");

            result.Add(new Template(name, template));
        }

        return result;
    }
}