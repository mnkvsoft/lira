using System.Text;
using Lira.Common;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

using static Consts.ExecutedBlock;

internal static class PatternParser
{
    public static PatternParts Parse(IReadOnlyCollection<string> patterns)
        => Parse(string.Join('\n', patterns));

    public static PatternParts Parse(string pattern)
    {
        var parts = new List<PatternPart>();
        var staticPart = new StringBuilder();

        var iterator = new StringIterator(pattern);
        var writeCallChain = false;

        while (iterator.MoveNext())
        {
            if (iterator.Current != BeginChar)
            {
                staticPart.Append(iterator.Current);
            }
            else
            {
                iterator.MoveNext();
                if (iterator.Current == BeginChar)
                {
                    if (staticPart.Length > 0)
                    {
                        parts.Add(new PatternPart.Static(staticPart.ToString()));
                        staticPart.Clear();
                    }

                    writeCallChain = true;
                    var dynamicPart = new StringBuilder();
                    while (iterator.MoveNext())
                    {
                        if (iterator.Current == EndChar)
                        {
                            iterator.MoveNext();
                            if (iterator.Current == EndChar)
                            {
                                writeCallChain = false;
                                break;
                            }
                            else
                            {
                                dynamicPart.Append(EndChar);
                                dynamicPart.Append(iterator.Current);
                            }
                        }
                        else
                            dynamicPart.Append(iterator.Current);
                    }

                    parts.Add(new PatternPart.Dynamic(dynamicPart.ToString()));
                }
                else
                {
                    staticPart.Append(BeginChar);
                    staticPart.Append(iterator.Current);
                }
            }
        }

        if (writeCallChain)
            throw new Exception($"Open block {{{{ not close in pattern '{pattern}'");

        if(staticPart.Length > 0)
            parts.Add(new PatternPart.Static(staticPart.ToString()));

        return new PatternParts(parts);
    }


}
