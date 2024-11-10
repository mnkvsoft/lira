using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.Custom.CustomDicModel;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.CustomSets;

static class CustomDictsLoader
{
    public static async Task<CustomDicts> Load(string path)
    {
        var templateFiles = DirectoryHelper.GetFiles(path, "*.dic");
        var result = new CustomDicts();

        foreach (var file in templateFiles)
        {
            var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(file));
            result.Add(file.GetFileName().TrimEnd(".dic"), lines);
        }

        return result;
    }
}