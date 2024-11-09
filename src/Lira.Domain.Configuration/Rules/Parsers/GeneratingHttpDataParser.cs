using Lira.Domain.Generating;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

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
        var patterns = PatternParser.Parse(block.Lines);
        var headersLines = patterns.Split(Common.Constants.NewLine);

        foreach (var line in headersLines)
        {
            var (headerName, headerPattern) = line.SplitToTwoParts(":").Trim();

            if (headerPattern == null)
                throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

            var headerParts = await _partsParser.Parse(headerPattern.ToString(), parsingContext);
            headers.Add(new GeneratingHeader(headerName.SingleStaticValueToString(), headerParts.WrapToTextParts()));
        }
        return headers;
    }

    public Task<ObjectTextParts> ParseText(string text, IParsingContext parsingContext)
    {
        return _partsParser.Parse(text, parsingContext);
    }
}
