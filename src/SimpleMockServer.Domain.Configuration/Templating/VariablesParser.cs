using SimpleMockServer.Common.Extensions;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Templating;

static class TemplatesLoader
{
    public static async Task<TemplateSet> Load(string path)
    {
        var templateFiles = DirectoryHelper.GetFiles(path, "*.templates");
        var result = new TemplateSet();

        foreach (var file in templateFiles)
        {
            var lines = TextCleaner.DeleteEmptiesAndComments( await File.ReadAllTextAsync(file));
            result.AddRange(TemplatesParser.Parse(lines));
        }

        return result;
    }
}

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
