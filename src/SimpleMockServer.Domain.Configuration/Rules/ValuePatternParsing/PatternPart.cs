namespace SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;

abstract record PatternPart(string Value)
{
    public record Static(string Value) : PatternPart(Value);
    public record Dynamic(string Value) : PatternPart(Value);
}
