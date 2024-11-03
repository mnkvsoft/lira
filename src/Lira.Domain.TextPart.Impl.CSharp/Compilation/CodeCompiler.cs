using System.Reflection;
using Lira.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record UsageAssemblies(IReadOnlyCollection<Assembly> Compiled, IReadOnlyCollection<PeImage> Runtime);

record PeImage(byte[] Bytes);

static class CodeCompiler
{
    public static PeImage Compile(IReadOnlyCollection<string> codes, string assemblyName, UsageAssemblies? usageAssemblies = null)
    {
        var compilation = CreateCompilation(codes, usageAssemblies, assemblyName);

        using var ms = new MemoryStream();

        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
            throw CreateException(codes, emitResult);

        ms.Seek(0, SeekOrigin.Begin);

        var bytes = ms.ToArray();
        return new PeImage(bytes);
    }

    private static CSharpCompilation CreateCompilation(IReadOnlyCollection<string> codes, UsageAssemblies? usageAssemblies,
        string? assemblyName)
    {
        var syntaxTrees = codes.Select(code => CSharpSyntaxTree.ParseText(code));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: syntaxTrees,
            references: GetReferences(usageAssemblies),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation;
    }

    private static Exception CreateException(IReadOnlyCollection<string> codes, EmitResult result)
    {
        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);


        var code = string.Join(new string(Enumerable.Repeat('-', 30).ToArray()), codes);
        foreach (var diagnostic in failures)
        {
            return new Exception($"Failed to compile. Error: {diagnostic.Id} {diagnostic.GetMessage()}. Code:{Constants.NewLine}'{code}'");
        }

        return new Exception($"Unknown error while compiling code '{code}'");
    }

    private static IReadOnlyCollection<MetadataReference> GetReferences(UsageAssemblies? usageAssemblies)
    {
        //The location of the .NET assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        if (assemblyPath == null)
            throw new Exception("Assembly path is null");

        var result = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonDocument).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Memory.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Security.Cryptography.dll")),
        };

        if (usageAssemblies != null)
        {
            result.AddRange(usageAssemblies.Compiled.Select(a => MetadataReference.CreateFromFile(a.Location)));
            result.AddRange(usageAssemblies.Runtime.Select(peImage => MetadataReference.CreateFromImage(peImage.Bytes)));
        }

        return result;
    }
}
