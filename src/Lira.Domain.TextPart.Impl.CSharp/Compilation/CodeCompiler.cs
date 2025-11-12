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

        // if (extractAdditionInfo != null)
        // {
        //     var semanticModel = compilation.GetSemanticModel(syntaxTrees.First(), true);
        //
        //     var methods = syntaxTrees
        //         .SelectMany(x => x.GetRoot().DescendantNodes())
        //         .OfType<MethodDeclarationSyntax>()
        //         .ToArray();
        //
        //     var first = methods.First();
        //     var nodes = first.DescendantNodes().ToArray();
        //     var innerMethods = nodes.OfType<LocalFunctionStatementSyntax>().ToArray();
        //     var method = innerMethods.FirstOrDefault(x => x.Identifier.ValueText == "GetInternal");
        //
        //     if (method != null)
        //     {
        //         var symbols = GetActualReturnTypes(semanticModel, method);
        //         variableType = symbols.First().ToDisplayString(Format);
        //     }
        //
        //
        //     // var symbolInfo = semanticModel.GetSymbolInfo(varResult.Declaration.Type);
        //     // var symbol = symbolInfo.Symbol;
        //     // variableType = symbol?.ToDisplayString(Format);
        // }

        return new Result.Success(new PeBytes(bytes), AdditionInfo: extractAdditionInfo?.Invoke(compilation));
    }

    private static IReadOnlyCollection<ITypeSymbol> GetActualReturnTypes(SemanticModel semanticModel, LocalFunctionStatementSyntax method)
    {
        var returnTypes = new List<ITypeSymbol>();

        var returnStatements = method.DescendantNodes().OfType<YieldStatementSyntax>().ToArray();

        foreach (var returnStatement in returnStatements)
        {
            if (returnStatement.Expression != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(returnStatement.Expression);
                if (typeInfo.Type != null)
                {
                    returnTypes.Add(typeInfo.Type);
                }
            }
        }

        return returnTypes;
    }

    private static readonly SymbolDisplayFormat Format = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

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

    // public class ReturnTypeAnalyzer
    // {
    //     public void Analyze(SyntaxTree tree)
    //     {
    //         var compilation = CSharpCompilation.Create("Analysis").AddSyntaxTrees(tree);
    //         var semanticModel = compilation.GetSemanticModel(tree);
    //
    //         var methods = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
    //
    //         foreach (var method in methods)
    //         {
    //             if (IsDynamicReturnMethod(method))
    //             {
    //                 var actualReturnTypes = GetActualReturnTypes(semanticModel, method);
    //                 Console.WriteLine($"Method {method.Identifier}: returns {string.Join(", ", actualReturnTypes)}");
    //             }
    //         }
    //     }
    //
    //     private bool IsDynamicReturnMethod(MethodDeclarationSyntax method)
    //     {
    //         return method.ReturnType is IdentifierNameSyntax identifier &&
    //                identifier.Identifier.Text == "dynamic";
    //     }
    //
    //
    // }
}
