namespace extensions;

public record CustomClass;

public static class CustomClassExtensions
{
    public static string ExtensionMethodInvoke(this CustomClass instance)
    {
        return "Hello from extension method!";
    }
}