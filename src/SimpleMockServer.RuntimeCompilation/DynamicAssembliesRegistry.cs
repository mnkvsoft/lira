using System.Reflection;
using System.Runtime.Loader;

namespace SimpleMockServer.RuntimeCompilation;

public class DynamicAssembliesRegistry
{
    private static int RevisionCounter;
    
    public int Revision { get; }
    private readonly AssemblyLoadContext _context = new(null);

    public DynamicAssembliesRegistry()
    {
        Revision = Interlocked.Increment(ref RevisionCounter);
    }

    public Assembly Load(CompileResult compileResult)
    {
        using var stream = new MemoryStream();
        stream.Write(compileResult.PeImage);
        stream.Position = 0;
        return _context.LoadFromStream(stream);
    }
}
