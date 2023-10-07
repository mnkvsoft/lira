namespace Lira.FileSectionFormat;

public static class TextCleaner
{
    public static IReadOnlyList<string> DeleteEmptiesAndComments(string text)
    {
        return Comments.Delete(text).DeleteEmpties();
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
