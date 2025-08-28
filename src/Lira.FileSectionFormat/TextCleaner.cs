using Lira.Common.Extensions;

namespace Lira.FileSectionFormat;

public static class TextCleaner
{
    public static IReadOnlyList<string> DeleteEmptiesAndComments(string text) => text.DeleteComments().DeleteEmpties();

    public static IReadOnlyList<string> DeleteComments(this string text)
    {
        var withoutComments = Comments.Delete(text.Replace("\r\n", "\n"));

        // split into lines in a non-OS specific format
        return withoutComments.Split("\n").TrimEmptyLines().ToArray();
    }

    private static IReadOnlyList<string> DeleteEmpties(this IReadOnlyCollection<string> lines)
    {
        var result = new List<string>(lines.Count);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            result.Add(line);
        }

        return result;
    }
}
