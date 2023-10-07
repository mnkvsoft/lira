namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

abstract record PatternPart
{
    public record Static(string Value) : PatternPart;
    public record Dynamic(string Value) : PatternPart;
}
