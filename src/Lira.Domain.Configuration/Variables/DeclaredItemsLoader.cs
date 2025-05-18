using Lira.FileSectionFormat;
using NuGet.Packaging;

namespace Lira.Domain.Configuration.Variables;

internal class DeclaredItemsLoader(DeclaredItemsParser parser)
{
    public async Task<IReadOnlyCollection<DeclaredItemDraft>> ReadDrafts(string path)
    {
        var result = new HashSet<DeclaredItemDraft>();

        foreach (var variableFile in DirectoryHelper.GetFiles(path, "*.declare"))
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(variableFile));
                var drafts = parser.Parse(lines, variableFile);

                result.AddRange(drafts);
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }

        return result;
    }
}
