using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SimpleMockServer.Domain.TextPart.CSharp;

static class DynamicClassLoader
{
    public static Assembly Compile(string code, params Assembly[] usageAssemblies)
    {
        var compilation = CreateCompilation(code, usageAssemblies);

        using var ms = new MemoryStream();
        
        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
            throw CreateException(code, emitResult);
        
        ms.Seek(0, SeekOrigin.Begin);

        var assembly = Assembly.Load(ms.ToArray());
        
        return assembly;
    }

    private static CSharpCompilation CreateCompilation(string code, Assembly[] usageAssemblies)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create(
            Path.GetRandomFileName(),
            syntaxTrees: new[] { syntaxTree },
            references: GetReferences(usageAssemblies),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        
        return compilation;
    }

    private static Exception CreateException(string code, EmitResult result)
    {
        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        foreach (var diagnostic in failures)
        {
            return new Exception($"Failed to compile. Error: {diagnostic.Id} {diagnostic.GetMessage()}. Code:{Environment.NewLine}'{code}'");
        }

        return new Exception($"Unknown error while compiling code '{code}'");
    }

    private static IReadOnlyCollection<MetadataReference> GetReferences(Assembly[] usageAssemblies)
    {
        //The location of the .NET assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        if (assemblyPath == null)
            throw new Exception("Assembly path is null");
        
        var result = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
        };

        result.AddRange(usageAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)));

        return result;
    }
}
