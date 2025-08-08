using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext);
}

class TextPartsParser(OperatorParser operatorParser, TextPartsParserInternal parserInternal, OperatorPartFactory operatorPartFactory) : ITextPartsParser
{
    public async Task<ObjectTextParts> Parse(string pattern, IParsingContext parsingContext)
    {
        // with local variables context
        var localContext = new ParsingContext(parsingContext.ToImpl());

        var tokens = operatorParser.Parse(pattern);

        var result = new List<IObjectTextPart>();
        result.AddRange(await parserInternal.Parse(tokens, localContext, operatorPartFactory));

        var ctx = parsingContext.ToImpl();
        ctx.DeclaredItems.TryAddRange(localContext.DeclaredItems);

        bool isString = result.Count is 0 or > 1;
        return new ObjectTextParts(result, isString);
    }
}