using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record UsageAssemblies(IReadOnlyCollection<PeImage> Runtime, IReadOnlyCollection<string> AssembliesLocations);

record PeImage(byte[] Bytes);

static class CodeCompiler
{
    public static CompileResult Compile(IReadOnlyCollection<string> codes, string assemblyName, UsageAssemblies? usageAssemblies = null)
    {
        var compilation = CreateCompilation(codes, usageAssemblies, assemblyName);

        using var ms = new MemoryStream();

        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
            return new CompileResult.Fault(CreateFaultMessage(emitResult));

        ms.Seek(0, SeekOrigin.Begin);

        var bytes = ms.ToArray();
        return new CompileResult.Success(new PeImage(bytes));
    }

    private static CSharpCompilation CreateCompilation(IReadOnlyCollection<string> codes, UsageAssemblies? usageAssemblies,
        string? assemblyName)
    {
        var syntaxTrees = codes.Select(code => CSharpSyntaxTree.ParseText(code)).ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: syntaxTrees,
            references: GetReferences(usageAssemblies),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTrees.First(), true);

        var varResult = syntaxTrees.First().GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(x => x.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            .FirstOrDefault(x => x.ToString().Contains("var __result"));

        if (varResult != null)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(varResult.Declaration.Type);
            var typeSymbol = symbolInfo.Symbol; // the type symbol for the variable..
        }

        // foreach (var member in members)
        // {
        //     var property = member as PropertyDeclarationSyntax;
        //     if (property != null)
        //         Console.WriteLine("Property: " + property.Identifier);
        //     var method = member as MethodDeclarationSyntax;
        //     if (method != null)
        //     {
        //         var variableDeclarations = method
        //             .DescendantNodes()
        //             .OfType<LocalDeclarationStatementSyntax>();
        //
        //         foreach (var variableDeclaration in variableDeclarations)
        //         {
        //             var symbolInfo = semanticModel.GetSymbolInfo(variableDeclaration.Declaration.Type);
        //             var typeSymbol = symbolInfo.Symbol; // the type symbol for the variable..
        //         }
        //
        //         var meth4 = method.DescendantNodes().ToArray();
        //         var meth = method.DescendantNodes()
        //             .OfType<LocalDeclarationStatementSyntax>()
        //             // .FirstOrDefault(x => x.ToString().StartsWith("var __result"));
        //             .FirstOrDefault(x => x.ToString().StartsWith("var __result"));
        //
        //
        //         var meth2 = method.DescendantNodes()
        //             // .OfType<LocalDeclarationStatementSyntax>()
        //             // .FirstOrDefault(x => x.ToString().StartsWith("var __result"));
        //             .FirstOrDefault(x => x.ToString().Contains("return"));
        //
        //
        //         if (meth != null)
        //         {
        //             string? typeName = semanticModel.GetTypeInfo(meth).Type?.Name ?? "none:(";
        //             // var typeInfo = semanticModel.GetTypeInfo(meth2);
        //             //
        //             // var symbolInfo =
        //             //     semanticModel.GetSymbolInfo(meth2);
        //         }
        //
        //     }
        // }

        return compilation;
    }

    private static string CreateFaultMessage(EmitResult result)
    {
        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine("Errors: ");


        if (!failures.Any())
        {
            sb.AppendLine("- unknown error while compiling code");
        }
        else
        {
            foreach (var diagnostic in failures)
            {
                sb.AppendLine($"- {diagnostic.Id} {diagnostic.GetMessage()}");
            }
        }

        return sb.ToString();
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
            MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableDictionary).Assembly.Location),
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
            result.AddRange(usageAssemblies.AssembliesLocations.Select(location =>  MetadataReference.CreateFromFile(location)));
            result.AddRange(usageAssemblies.Runtime.Select(peImage => MetadataReference.CreateFromImage(peImage.Bytes)));
        }

        return result;
    }
}
