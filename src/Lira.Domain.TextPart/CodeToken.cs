namespace Lira.Domain.TextPart;

public abstract record CodeToken(string RawValue)
{
    public record WriteItem(string ItemName) : CodeToken(ItemName)
    {
        public override string ToString() => $"[:w {ItemName}]";
    }
    public record ReadItem(string ItemName) : CodeToken(ItemName)
    {
        public override string ToString() => $"[:r {ItemName}]";
    }
    public record OtherCode(string Code) : CodeToken(Code)
    {
        public override string ToString() => $"[:c {Code}]";
    }
}