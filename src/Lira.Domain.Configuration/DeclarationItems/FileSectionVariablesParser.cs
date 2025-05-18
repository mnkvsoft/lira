using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.DeclarationItems;

class FileSectionDeclaredItemsParser(DeclaredItemsLinesParser linesParser, DeclaredItemDraftsParser draftsParser)
{
    public Task<DeclaredItems> Parse(FileSection variablesSection, ParsingContext parsingContext)
    {
        var drafts = linesParser.Parse(variablesSection.LinesWithoutBlock);
        return draftsParser.Parse(drafts, parsingContext);
    }
}
