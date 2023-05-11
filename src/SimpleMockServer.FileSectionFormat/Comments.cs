using System.Text;

namespace SimpleMockServer.FileSectionFormat;

public static class Comments
{
    static class CommentChar
    {
        public const string SingleLine = "//";
        public const string MultiLineStart = "/*";
        public const string MultiLineEnd = "*/";
    }
    
    public static string Delete(string text)
    {
        text = Delete(text, " " + CommentChar.SingleLine, Environment.NewLine, isMultiline: false);
        text = Delete(text, CommentChar.MultiLineStart, CommentChar.MultiLineEnd, isMultiline: true);

        var lines = text.Split(Environment.NewLine)
            .Where(l => !l.StartsWith(CommentChar.SingleLine));

        text = string.Join(Environment.NewLine, lines);
        
        return text;
    }

    private static string Delete(string text, string startComment, string endComment, bool isMultiline)
    {
        var result = new StringBuilder();

        int startCommentIndex = text.IndexOf(startComment, StringComparison.OrdinalIgnoreCase);
        while (startCommentIndex != -1)
        {
            result.Append(text.Substring(0, startCommentIndex));
            text = text.Substring(startCommentIndex);

            int endCommentIndex = text.IndexOf(endComment, StringComparison.OrdinalIgnoreCase);
            if (endCommentIndex == -1)
                break;

            text = text.Substring(isMultiline ? endCommentIndex + endComment.Length : endCommentIndex);
            startCommentIndex = text.IndexOf(startComment, StringComparison.OrdinalIgnoreCase);
        }

        result.Append(text);
        return result.ToString();
    }
}
