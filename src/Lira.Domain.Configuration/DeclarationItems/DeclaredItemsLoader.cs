using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
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
                var items = linesParser.Parse(lines);

                var variables = items.Where(i => i.Name.StartsWith(RuleVariable.Prefix)).ToArray();

                if(variables.Length != 0)
                    throw new Exception($"It is forbidden to declare variables inside .declare. Variables: {string.Join(", ", variables.Select(x => x.Name))}. File: {declarationFile}");

                drafts.AddRange(items);
            }
            catch (Exception exc)
            {
                throw new FileParsingException(declarationFile, exc);
            }
        }

        return await draftsParser.Parse(drafts, parsingContext);
    }
}