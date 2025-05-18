using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;
using NuGet.Packaging;

namespace Lira.Domain.Configuration.DeclarationItems;

internal class DeclaredItemsLoader(DeclaredItemsLinesParser linesParser, DeclaredItemDraftsParser draftsParser)
{
    public async Task<DeclaredItems> Load(ParsingContext parsingContext, string path)
    {
        var drafts = new HashSet<DeclaredItemDraft>();

        foreach (var declarationFile in DirectoryHelper.GetFiles(path, "*.declare"))
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(declarationFile));
                drafts.AddRange(linesParser.Parse(lines));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(declarationFile, exc);
            }
        }

        return await draftsParser.Parse(drafts, parsingContext);
    }
}