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

        if (tokens.Any(x => x is Token.Operator))
            return new ObjectTextParts([new AddIndentsWrapper(result)], isString: true);

        bool isString = tokens.Count == 0 || tokens.Count > 1 || (tokens[0] is Token.StaticData sd && sd.Content[0] is PatternPart.Static);
        return new ObjectTextParts(result, isString);
    }

    class AddIndentsWrapper(List<IObjectTextPart> parts) : IObjectTextPart
    {
        public IEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            int countIndent = 0;
            string? lastStr = null;

            foreach (var part in parts)
            {
                if (part is OperatorPart)
                {
                    bool isFirst = true;
                    foreach (var obj in part.Get(context))
                    {
                        string? str = ObjectTextPartsExtensions.GetStringValue(obj);

                        if (isFirst)
                        {
                            isFirst = false;
                            lastStr = str;
                            yield return str;
                        }
                        else
                        {
                            if(lastStr != null && lastStr.EndsWith('\n'))
                                yield return new string(' ', countIndent);

                            lastStr = str;
                            yield return str;
                        }
                    }

                    countIndent = 0;
                }
                else
                {
                    foreach (var obj in part.Get(context))
                    {
                        string? str = ObjectTextPartsExtensions.GetStringValue(obj);

                        if(str == null)
                            continue;

                        int nlIndex = str.LastIndexOf('\n');
                        if (nlIndex != -1)
                        {
                            countIndent = str.Length - nlIndex - 1;
                        }
                        else
                        {
                            countIndent += str.Length;
                        }

                        lastStr = str;
                        yield return str;
                    }
                }
            }
        }

        public ReturnType ReturnType => ReturnType.String;
    }
}