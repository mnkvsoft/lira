using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules.Parsers;

public class HeadersParser(ITextPartsParser textPartsParser)
{
    public async Task<IReadOnlyCollection<GeneratingHeader>> ParseHeaders(FileBlock block, IParsingContext parsingContext)
    {
        var headers = new List<GeneratingHeader>();
        var patterns = PatternParser.Parse(block.Lines);
        var headersLines = patterns.Split(Common.Constants.NewLine);

        foreach (var line in headersLines)
        {
            var (headerName, headerPattern) = line.SplitToTwoParts(":").Trim();

            var headerParts = headerPattern == null
                ? null
                : await textPartsParser.Parse(headerPattern.ToString(), parsingContext);

            headers.Add(new GeneratingHeader(
                headerName.SingleStaticValueToString(),
                headerParts?.WrapToTextParts() ?? TextPartsProvider.Empty));
        }

        return headers;
    }
}
