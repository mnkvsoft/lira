using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Variables;

internal class DeclaredItemsLoader(DeclaredItemsParser parser)
{
    public async Task<DeclaredItems> Load(IReadonlyParsingContext parsingContext, string path)
    {
        var result = new DeclaredItems();
        var newContext = new ParsingContext(parsingContext, declaredItems: result);

        foreach (var variableFile in DirectoryHelper.GetFiles(path, "*.declare"))
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(variableFile));
                result.Add(await parser.Parse(lines, newContext.WithCurrentPath(variableFile.GetDirectory())));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }
        return result;
    }
}
