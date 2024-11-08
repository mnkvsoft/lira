namespace Lira.Common.Extensions;

public static class StringCollectionExtensions
{
    public static string JoinWithNewLine(this IReadOnlyCollection<string> lines) => string.Join('\n', lines);
}