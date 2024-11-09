namespace chain_second_block_is_csharp;

public static class Cleaner
{
    public static string Clean(string value)
    {
        ArgValidation.Arg.Empty(value, nameof(value));
        return value.Replace(" ", string.Empty);
    }
}
