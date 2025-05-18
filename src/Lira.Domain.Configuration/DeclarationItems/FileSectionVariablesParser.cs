using Lira.Domain.Configuration.DeclarationItems;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Variables;

class FileSectionDeclaredItemsParser
{
    private readonly DeclaredItemsParser _parser;

    public FileSectionDeclaredItemsParser(DeclaredItemsParser parser) => _parser = parser;

    public Task<DeclaredItems> Parse(FileSection variablesSection, IReadonlyParsingContext parsingContext)
        => _parser.Parse(variablesSection.LinesWithoutBlock, parsingContext);
}
