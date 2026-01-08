using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.Parsers;

public class RuleKeyExtractorParser(ITextPartsParser textPartsParser)
{
    public async Task<IRuleKeyExtractor> Parse(string pattern, IParsingContext parsingContext)
    {
       var parts = await textPartsParser.Parse(pattern, parsingContext);
       return new RuleKeyExtractor(parts.WrapToTextParts());
    }
}