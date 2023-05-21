namespace Custom;

public static class Cleaner
{ 
    public static string Clean(string value)
    {
        return value.Replace(" ", string.Empty);
    }
}
