using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record CompileUnit(
    IReadOnlyCollection<string> Codes,
    string AssemblyName,
    UsageAssemblies? UsageAssemblies)
{
    private Hash? _hash;

    public Hash GetHash()
    {
        return _hash ??= GetHash(this);
    }

    private static Hash GetHash(CompileUnit compileUnit)
    {
        using var memoryStream = new MemoryStream();
        using var sw = new StreamWriter(memoryStream);

        sw.Write(compileUnit.AssemblyName);

        foreach (var code in compileUnit.Codes)
        {
            sw.Write(code);
        }

        var usageAssemblies = compileUnit.UsageAssemblies;
        if (usageAssemblies != null)
        {
            foreach (var peImage in usageAssemblies.Runtime)
            {
                sw.Write(Sha1.Create(peImage.Bytes));
            }

            foreach (var location in usageAssemblies.AssembliesLocations)
            {
                sw.Write(location);
            }
        }

        sw.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var hash = Sha1.Create(memoryStream);
        return hash;
    }
}