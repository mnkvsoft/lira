using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Generating;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Transform.Format;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

public class GeneratingHttpDataParser
{
    private readonly ITextPartsParser _partsParser;

    public GeneratingHttpDataParser(ITextPartsParser textGeneratorFactory)
    {
        _partsParser = textGeneratorFactory;
    }

    public async Task<IReadOnlyCollection<GeneratingHeader>> ParseHeaders(FileBlock block, IParsingContext parsingContext)
    {
        var headers = new List<GeneratingHeader>();
        foreach (var line in block.Lines)
        {
            if (string.IsNullOrEmpty(line))
                break;

            var (headerName, headerPattern) = line.SplitToTwoParts(":").Trim();

            if (headerPattern == null)
                throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

            var parts = await _partsParser.Parse(headerPattern, parsingContext);

            headers.Add(new GeneratingHeader(headerName, ObjectTextPartsExtensions.WrapToTextParts(parts)));
        }
        return headers;
    }

    public Task<ObjectTextParts> ParseBody(FileBlock block, IParsingContext parsingContext)
    {
        return _partsParser.Parse(block.GetStringValue(), parsingContext);
    }
}
