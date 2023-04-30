using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

public class GeneratingHttpDataParser
{
    private readonly ITextPartsParser _partsParser;

    public GeneratingHttpDataParser(ITextPartsParser textGeneratorFactory)
    {
        _partsParser = textGeneratorFactory;
    }

    public IReadOnlyCollection<GeneratingHeader> ParseHeaders(FileBlock block, VariableSet variables)
    {
        var headers = new List<GeneratingHeader>();
        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            (string headerName, string? headerPattern) = line.SplitToTwoParts(":").Trim();

            if (headerPattern == null)
                throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

            var textGenerator = _partsParser.Parse(headerPattern, variables);

            headers.Add(new GeneratingHeader(headerName, textGenerator));
        }
        return headers;
    }

    public TextParts ParseBody(FileBlock block, VariableSet variables)
    {
        return _partsParser.Parse(block.GetStringValue(), variables);
    }
}
