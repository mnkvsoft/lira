using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.CustomDictionaries;

static class CustomDictsLoader
{
    public static async Task<IReadOnlyDictionary<string, CustomDic>> Load(string path)
    {
        var templateFiles = DirectoryHelper.GetFiles(path, "*.dic");
        var result = new Dictionary<string, CustomDic>();

        foreach (var file in templateFiles)
        {
            var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(file));
            result.Add(Path.GetFileName(file).TrimEnd(".dic"), new CustomDic(lines));
        }

        return result;
    }
}