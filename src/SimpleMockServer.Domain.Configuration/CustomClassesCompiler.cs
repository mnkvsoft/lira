using SimpleMockServer.RuntimeCompilation;

namespace SimpleMockServer.Domain.Configuration;

class CustomClassesCompiler
{
    public async Task<CompileResult?> Compile(string path, string assemblyName)
    {
        var csharpFiles = DirectoryHelper.GetFiles(path, "*.cs");

        if(csharpFiles.Count == 0)
            return null;

        var tasks = csharpFiles.Select(f => File.ReadAllTextAsync(f)).ToList();
        await Task.WhenAll(tasks);

        var codes = tasks.Select(x => x.Result).ToArray();

        return DynamicClassLoader.Compile(codes, assemblyName);
    }
}
