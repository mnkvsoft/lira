using System.Reflection;
using System.Runtime.Loader;
using SimpleMockServer.RuntimeCompilation;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public class CSharpCodeRegistry
{
    public record CustomAssembly1(Assembly Assembly, byte[] PeImage);

    private readonly AssemblyLoadContext _context = new(null);

    public CustomAssembly1? CustomAssembly { get; private set; }

    public int Revision { get; }

    public CSharpCodeRegistry(int revision)
    {
        Revision = revision;
    }

    public void LoadCustomAssembly(CompileResult compileResult)
    {
        CustomAssembly = new CustomAssembly1(Load(compileResult), compileResult.PeImage);
    }

    public Assembly Load(CompileResult compileResult)
    {
        using var stream = new MemoryStream();
        stream.Write(compileResult.PeImage);
        stream.Position = 0;
        return _context.LoadFromStream(stream);
    }

}
