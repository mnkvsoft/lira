namespace Lira.Domain.Configuration.Rules.Parsers.CodeParsing;

public abstract record CodeToken
{
    public record WriteItem(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:w {ItemName}]";
    }
    public record ReadItem(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:r {ItemName}]";
    }
    public record OtherCode(string Code) : CodeToken
    {
        public override string ToString() => $"[:c {Code}]";
    }
}