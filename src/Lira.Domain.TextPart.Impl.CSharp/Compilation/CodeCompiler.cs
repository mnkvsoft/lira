using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

static class CodeCompiler
{
    public abstract record Result
    {
        public record Success(PeBytes PeBytes, string? AdditionInfo) : Result;
        public record Fault(string Message) : Result;
    }

    public static Result Compile(string assemblyName, IReadOnlyCollection<MetadataReference> references, IEnumerable<SyntaxTree> syntaxTrees, Func<CSharpCompilation, string>? extractAdditionInfo = null)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();

        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
            return new Result.Fault(CreateFaultMessage(emitResult));

        ms.Seek(0, SeekOrigin.Begin);

        var bytes = ms.ToArray();

        return new Result.Success(new PeBytes(bytes), AdditionInfo: extractAdditionInfo?.Invoke(compilation));
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
}
