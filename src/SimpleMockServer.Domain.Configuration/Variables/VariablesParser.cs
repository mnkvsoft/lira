using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Variables;

class VariablesParser
{
    private readonly ITextPartsParser _textPartsParser;

    public VariablesParser(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public async Task<VariableSet> Parse(FileSection variablesSection, ParsingContext parsingContext)
    {
        var set = new VariableSet(parsingContext.Variables);

        foreach (var line in variablesSection.LinesWithoutBlock)
        {
            var (name, pattern) = line.SplitToTwoParts("=").Trim();

            if (string.IsNullOrEmpty(name))
                throw new Exception($"RequestVariable name not defined. Line: {line}");

            if (string.IsNullOrEmpty(pattern))
                throw new Exception($"RequestVariable '{name}' not initialized. Line: {line}");

            var parts = await _textPartsParser.Parse(pattern, parsingContext with {Variables = set} );
            set.Add(new RequestVariable(name, parts));
        }

        return set;
    }
}
