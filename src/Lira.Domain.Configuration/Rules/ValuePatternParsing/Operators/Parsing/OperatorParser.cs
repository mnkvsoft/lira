using Lira.Domain.Configuration.Parsing;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class OperatorParser(IEnumerable<IOperatorHandler> handlers)
{
    private readonly TokenParser _tokenParser = new(handlers.Select(x => x.Definition));

    public IReadOnlyList<Token> Parse(string text) => _tokenParser.Parse(text);
}