namespace Lira.FileSectionFormat;

public static class TextCleaner
{
    public static IReadOnlyList<string> DeleteEmptiesAndComments(string text)
    {
        var withoutComments = Comments.Delete(text);

        // split into lines in a non-OS specific format
        var lines = withoutComments.Replace("\r\n", "\n").Split("\n").ToArray();
        return lines.DeleteEmpties();
    }

    private static IReadOnlyList<string> DeleteEmpties(this IReadOnlyCollection<string> lines)
    {
        var result = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            result.Add(line);
        }

        return result;
    }
}
