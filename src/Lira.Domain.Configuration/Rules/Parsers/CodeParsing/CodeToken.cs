namespace Lira.Domain.Configuration.Rules.Parsers.CodeParsing;

public abstract record CodeToken
{
    public record WriteVariable(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:w {ItemName}]";
    }
    public record ReadVariable(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:r {ItemName}]";
    }
    public record OtherCode(string Code) : CodeToken
    {
        public override string ToString() => $"[:c {Code}]";
    }
}