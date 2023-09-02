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