namespace Lira.Common;

public static class JsonUtils
{
    public static bool MaybeValidJson(string str)
    {
        foreach (char c in str)
        {
            if(char.IsWhiteSpace(c))
                continue;

            if(c is '[' or '{')
                return true;

            return false;
        }

        return false;
    }
}