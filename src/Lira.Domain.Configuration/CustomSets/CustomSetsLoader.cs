using Lira.Common;
using Lira.Common.Extensions;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.CustomSets;

static class CustomSetsLoader
{
    public static async Task<TextPart.Impl.Custom.CustomSetModel.CustomSets> Load(string path)
    {
        var templateFiles = DirectoryHelper.GetFiles(path, "*.set");
        var result = new TextPart.Impl.Custom.CustomSetModel.CustomSets();

        foreach (var file in templateFiles)
        {
            var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(file));
            result.Add(file.GetFileName().TrimEnd(".set"), lines);
        }

        return result;
    }
}