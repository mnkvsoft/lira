using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Generating;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

public class GeneratingHttpDataParser
{
    private readonly ITextPartsParser _partsParser;

    public GeneratingHttpDataParser(ITextPartsParser textGeneratorFactory)
    {
        _partsParser = textGeneratorFactory;
    }

    public IReadOnlyCollection<GeneratingHeader> ParseHeaders(FileBlock block, IReadOnlyCollection<Variable> variables)
    {
        var headers = new List<GeneratingHeader>();
        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            var (headerName, headerPattern) = line.SplitToTwoParts(":").Trim();

            if (headerPattern == null)
                throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

            var parts = _partsParser.Parse(headerPattern, variables);

            headers.Add(new GeneratingHeader(headerName, parts.WrapToTextParts()));
        }
        return headers;
    }

    public ObjectTextParts ParseBody(FileBlock block, IReadOnlyCollection<Variable> variables)
    {
        return _partsParser.Parse(block.GetStringValue(), variables);
    }
}
