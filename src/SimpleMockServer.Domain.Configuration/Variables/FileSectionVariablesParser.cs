using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Variables;

class FileSectionVariablesParser
{
    private readonly VariablesParser _variablesParser;

    public FileSectionVariablesParser(VariablesParser variablesParser) => _variablesParser = variablesParser;

    public Task<VariableSet> Parse(FileSection variablesSection, ParsingContext parsingContext)
        => _variablesParser.Parse(variablesSection.LinesWithoutBlock, parsingContext, (name, parts) => new RequestVariable(name, parts));
}
