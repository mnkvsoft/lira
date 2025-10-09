using System.Text;

namespace Lira.Common.Extensions;

public static class ExceptionExtensions
{
    public static string GetMessagesChain(this Exception exception)
    {
        var sb = new StringBuilder();
        AddMessagesChain(exception, sb);
        return sb.ToString();
    }

    private static void AddMessagesChain(Exception exception, StringBuilder messageChain)
    {
        messageChain.AppendLine(exception.Message);

        var inner = exception.InnerException;
        if(inner != null)
            AddMessagesChain(inner, messageChain);
    }
}