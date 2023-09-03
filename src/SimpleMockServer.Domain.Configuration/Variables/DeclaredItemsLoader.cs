using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Variables;

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
        foreach (var variableFile in DirectoryHelper.GetFiles(path, "*.declare"))
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(variableFile));
                result.Add(await _parser.Parse(lines, parsingContext with { CurrentPath = variableFile.GetDirectory()}));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }
        return result;
    }
}
