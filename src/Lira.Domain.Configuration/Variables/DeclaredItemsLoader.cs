using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.FileSectionFormat;
using NuGet.Packaging;

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
                var items = await parser.Parse(lines, newContext.WithCurrentPath(variableFile.GetDirectory()));

                var invalidItems = items.Where(x => x is not Function).ToArray();

                if(invalidItems.Any())
                    throw new Exception("*.declare files may contain only function definitions. Invalid definitions: " + string.Join(", ", invalidItems.Select(x=> x.Name)));

                result.AddRange(items);
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }

        return result;
    }
}
