namespace Lira.Domain.Handling.Generating;

public abstract record WriteHistoryMode
{
    public record Write(RuleName RuleName) : WriteHistoryMode;

    public record None : WriteHistoryMode
    {
        public static readonly None Instance = new();
    }
}