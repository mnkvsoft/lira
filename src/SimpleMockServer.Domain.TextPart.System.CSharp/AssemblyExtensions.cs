using System.Reflection;

namespace SimpleMockServer.Domain.TextPart.System.CSharp;

internal static class AssemblyExtensions
{
    public static string ReadResource(this Assembly assembly, string name)
    {
        // Determine path
        var names = assembly.GetManifestResourceNames();
        string resourcePath = names.Single(str => str.EndsWith(name));

        using Stream stream = assembly.GetManifestResourceStream(resourcePath)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
