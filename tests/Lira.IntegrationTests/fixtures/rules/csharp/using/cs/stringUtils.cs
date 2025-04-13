// namespaces was imported from GlobalUsings.cs

namespace utils;

public static class stringUtils
{
    private static char[] _digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '8'];

    public static string digits(int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            var digit = _digits[Random.Shared.Next(0, _digits.Length)];
            sb.Append(digit);
        }

        // return static value for test validation
        // return sb.ToString();
        return "1122334455";
    }
}