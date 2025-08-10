using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

namespace Lira.Domain.Configuration.UnitTests;

static class TokensExtensions
{
    public static string GetXmlView(this IReadOnlyList<Token> tokens)
    {
        return string.Concat(tokens.Select(x => x.ToString()));
    }
}