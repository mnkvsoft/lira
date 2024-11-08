using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Variables;

internal class DeclaredItemsLoader
{
    private readonly DeclaredItemsParser _parser;

    public DeclaredItemsLoader(DeclaredItemsParser parser)
    {
        _parser = parser;
    }

    public async Task<IReadonlyDeclaredItems> Load(ParsingContext parsingContext, string path)
    {
        var result = new DeclaredItems();
        var newContext = parsingContext with { DeclaredItems = result };
        
        foreach (var variableFile in DirectoryHelper.GetFiles(path, "*.declare"))
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(variableFile));
                result.Add(await _parser.Parse(lines, newContext with { CurrentPath = variableFile.GetDirectory()}));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }
        return result;
    }
}
