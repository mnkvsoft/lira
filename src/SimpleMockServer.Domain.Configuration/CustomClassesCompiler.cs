using SimpleMockServer.RuntimeCompilation;

namespace SimpleMockServer.Domain.Configuration;

class CustomClassesCompiler
{
    public async Task<CompileResult?> Compile(string path)
    {
        var csharpFiles = DirectoryHelper.GetFiles(path, "*.cs");

        if(csharpFiles == null)
            return null;

        var tasks = csharpFiles.Select(f => File.ReadAllTextAsync(f)).ToList();
        await Task.WhenAll(tasks);

        var codes = tasks.Select(x=> x.Result).ToArray();

        var result = DynamicClassLoader.Compile(codes, assemblyName: "UserCustomClasses");
        return result;
    }
}
