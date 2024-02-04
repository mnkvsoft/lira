using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.Custom.CustomSetModel;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Templating;

static class CustomSetsLoader
{
    public static async Task<CustomSets> Load(string path)
    {
        var templateFiles = DirectoryHelper.GetFiles(path, "*.set");
        var result = new CustomSets();

        foreach (var file in templateFiles)
        {
            var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(file));
            result.Add(file.GetFileName().TrimEnd(".set"), lines);
        }

        return result;
    }
}