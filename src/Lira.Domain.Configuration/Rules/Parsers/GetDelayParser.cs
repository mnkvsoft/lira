using Lira.Common.PrettyParsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating;
using Lira.Domain.TextPart;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class GetDelayParser(ITextPartsParser partsParser)
{
    public async Task<GetDelay?> Parse(FileSection responseSection, ParsingContext parsingContext)
    {
        var block = responseSection.GetBlock(Constants.BlockName.Response.Delay);

        if (block == null)
            return null;

        var delayStr = block.GetSingleLine();

        if (!delayStr.Contains("{{"))
        {
            var delay = PrettyTimespanParser.Parse(delayStr);
            return _ => Task.FromResult(delay);
        }

        var parts = await partsParser.Parse(delayStr, parsingContext);
        var textParts = parts.WrapToTextParts();

        return GetDelay;

        async Task<TimeSpan> GetDelay(RuleExecutingContext ctx) => PrettyTimespanParser.Parse(await textParts.GetSingleString(ctx));
    }
}