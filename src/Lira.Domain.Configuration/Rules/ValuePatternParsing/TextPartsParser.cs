using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface ITextPartsParser
{
    Task<IObjectTextPart> Parse(string pattern, IParsingContext parsingContext);
}

class TextPartsParser(OperatorParser operatorParser, TextPartsParserInternal parserInternal, OperatorPartFactory operatorPartFactory) : ITextPartsParser
{
    public async Task<IObjectTextPart> Parse(string pattern, IParsingContext parsingContext)
    {
        // with local variables context
        var localContext = new ParsingContext(parsingContext.ToImpl());

        var tokens = operatorParser.Parse(pattern);

        var result = new List<IObjectTextPart>();
        result.AddRange(await parserInternal.Parse(tokens, localContext, operatorPartFactory));

        var ctx = parsingContext.ToImpl();
        ctx.DeclaredItems.TryAddRange(localContext.DeclaredItems);

        if (tokens.Any(x => x is Token.Operator))
            return new AddIndentsWrapper(result);

        if (tokens.Count == 0)
            throw new Exception($"Pattern '{pattern}' is empty");

        if (result.Count == 1)
            return result[0];

        return new EnumerableObjectTextPart(result);
    }

    class EnumerableObjectTextPart(IEnumerable<IObjectTextPart> parts) : IObjectTextPart
    {
        public dynamic Get(RuleExecutingContext context) => GetInternal(context);

        private IEnumerable<dynamic?> GetInternal(RuleExecutingContext context) => parts.GetEnumerable(context);

        public Type Type => DotNetType.EnumerableDynamic;
    }

    class AddIndentsWrapper(IEnumerable<IObjectTextPart> parts) : IObjectTextPart
    {
        public dynamic Get(RuleExecutingContext context) => GetInternal(context);

        public Type Type => DotNetType.EnumerableDynamic;

        private IEnumerable<string?> GetInternal(RuleExecutingContext context)
        {
            int countIndent = 0;
            string? lastStr = null;

            foreach (var part in parts)
            {
                if (part is OperatorPart)
                {
                    bool isFirst = true;
                    foreach (var obj in part.GetEnumerable(context))
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
                    foreach (var obj in part.GetEnumerable(context))
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

    }
}