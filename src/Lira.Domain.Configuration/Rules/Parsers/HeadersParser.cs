using Lira.Domain.Generating;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

public class HeadersParser(ITextPartsParser textPartsParser)
{
    public async Task<IReadOnlyCollection<GeneratingHeader>> ParseHeaders(FileBlock block, IReadonlyParsingContext parsingContext)
    {
        var headers = new List<GeneratingHeader>();
        var patterns = PatternParser.Parse(block.Lines);
        var headersLines = patterns.Split(Common.Constants.NewLine);

        foreach (var line in headersLines)
        {
            var (headerName, headerPattern) = line.SplitToTwoParts(":").Trim();

            if (headerPattern == null)
                throw new Exception($"Empty matching for header '{headerPattern}' in line: '{line}'");

            var headerParts = await textPartsParser.Parse(headerPattern.ToString(), parsingContext);
            headers.Add(new GeneratingHeader(headerName.SingleStaticValueToString(), headerParts.WrapToTextParts()));
        }

        return headers;
    }
}
