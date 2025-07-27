namespace Lira.Domain.Configuration.Parsing;

static class OperatorParser
{
    private static readonly TokenParser TokenParser = new(
    [
        new OperatorDefinition("repeat", ParametersMode.Maybe),
        new OperatorDefinition("random", ParametersMode.None,
            allowedChildElements: new Dictionary<string, ParametersMode> { { "item", ParametersMode.None } }),
        new OperatorDefinition("if", ParametersMode.Required,
            allowedChildElements: new Dictionary<string, ParametersMode>
            {
                { "else", ParametersMode.None },
                { "else if", ParametersMode.Required }
            })
    ]);

    public static IReadOnlyList<Token> Parse(string text) => TokenParser.Parse(text);
}