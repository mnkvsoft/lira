using System.Text;

namespace Lira.FileSectionFormat;

internal static class Comments
{
    static class CommentChar
    {
        public const string SingleLine = "@-";
        public const string MultiLineStart = "@*";
        public const string MultiLineEnd = "*@";
    }

    public static string Delete(string text)
    {
        text = Delete(text, CommentChar.MultiLineStart, CommentChar.MultiLineEnd, isMultiline: true);
        text = Delete(text, CommentChar.SingleLine, "\n", isMultiline: false);

        return text;
    }

    private static string Delete(string text, string startComment, string endComment, bool isMultiline)
    {
        var result = new StringBuilder();

        int startCommentIndex = text.IndexOf(startComment, StringComparison.OrdinalIgnoreCase);

        string? lastNotComment = null;
        while (startCommentIndex != -1)
        {
            lastNotComment = text.Substring(0, startCommentIndex);
            result.Append(lastNotComment);
            text = text.Substring(startCommentIndex + startComment.Length);

            int endCommentIndex = text.IndexOf(endComment, StringComparison.OrdinalIgnoreCase);
            if (endCommentIndex == -1)
            {
                text = "";
                break;
            }

            int idx;
            if (isMultiline)
            {
                idx = endCommentIndex + endComment.Length;
            }
            else
            {
                idx = endCommentIndex;
            }

            if (idx + 1 < text.Length && text[idx] == '\n')
            {
                if(lastNotComment.LastOrDefault() == '\n')
                    idx++;
            }

            text = text.Substring(idx);
            startCommentIndex = text.IndexOf(startComment, StringComparison.OrdinalIgnoreCase);
        }

        result.Append(text);
        return result.ToString();
    }
}
