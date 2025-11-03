using System.Diagnostics;
using Lira.Common;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lira.Domain.TextPart.Impl.CSharp;

partial class FunctionFactory
{
    public CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(
        FunctionFactoryRuleContext ruleContext,
        CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        const string resultVariableName = "__result";

        var sourceCode = code.ToString();
        var (toCompile, usings) = PrepareCode(code, ruleContext);

        // result type already known
        if (sourceCode.Contains("yield"))
        {
            toCompile = $$"""
                        return GetInternal();

                        IEnumerable<dynamic?> GetInternal()
                        {
                            {{toCompile}}
                        }
                        """;

            var (tree, names) =
                CreateGeneratingFunctionClassCode(
                    sourceCode,
                    toCompile,
                    functionReturnType: $"{nameof(DotNetType)}.{nameof(DotNetType.EnumerableDynamic)}",
                    usings);

            var res = CreateFunction<IObjectTextPart>(
                names: names,
                tree,
                extractAdditionInfo: null,
                CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

            _compilationStatistic.AddTotalTime(sw.Elapsed);

            return res;
        }

        {
            toCompile = EnrichReturnOperator(toCompile);

            // calc type
            var (syntaxTreeForCalcType, namesForCalcType) =
                CreateGeneratingFunctionClassCode(
                    sourceCode,
                    toCompile,
                    functionReturnType: $"{nameof(DotNetType)}.{nameof(DotNetType.Unknown)}",
                    usings);

            var resultForCalcType = CreateFunction<IObjectTextPart>(
                names: namesForCalcType,
                syntaxTreeForCalcType,
                semanticModel => GetReturnType(semanticModel, syntaxTreeForCalcType),
                CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

            if (resultForCalcType is not CreateFunctionResult<IObjectTextPart>.Success successResult)
                return resultForCalcType;

            var type = successResult.AdditionInfo ??
                       throw new Exception("The return type for the code could not be calculated: " + code);

            // compile real function
            var (syntaxTree, names) =
                CreateGeneratingFunctionClassCode(
                    sourceCode,
                    toCompile,
                    functionReturnType: type == "dynamic"
                        ? $"{nameof(DotNetType)}.{nameof(DotNetType.Unknown)}"
                        : $"typeof({type})",
                    usings);

            var result = CreateFunction<IObjectTextPart>(
                names: names,
                syntaxTree,
                extractAdditionInfo: null,
                CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

            _compilationStatistic.AddTotalTime(sw.Elapsed);

            return result;

            static string EnrichReturnOperator(string c)
            {
                if (c.Contains("return"))
                    return c;

                return $"var {resultVariableName} = {c};" + Constants.NewLine +
                       $"; return {resultVariableName};";
            }
        }
    }

     private (SyntaxTree, DynamicNames) CreateGeneratingFunctionClassCode(
         string sourceCode,
         string toCompile,
         string functionReturnType,
         IReadOnlyCollection<string> usings)
    {
        var classWithoutName = ClassCodeCreator.CreateIObjectTextPart(
            functionReturnType,
            GetMethodBody(
                new Code(
                    ForCompile: WrapToTryCatch(new Code(toCompile, sourceCode)),
                    Source: sourceCode)),
            ContextParameterName,
            ReservedVariable.Req,
            usings,
            GetNamespaces(),
            GetUsingStatic());

        var names = _namer.GetNames(prefix: "GeneratingFunction", classWithoutName);

        var syntaxTree = CSharpSyntaxTree.ParseText(classWithoutName.SetClassName(names.Class));

        return (syntaxTree, names);
    }

    string GetReturnType(CSharpCompilation compilation, SyntaxTree syntaxTree)
    {
        var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);

        var getMethod = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(x => x.Identifier.ValueText == nameof(IObjectTextPart.Get));

        var symbols = GetActualReturnTypes(semanticModel, getMethod);
        var returnTypes = symbols.Select(x => x.ToDisplayString(Format)).ToArray();

        var returnTypesUnique = returnTypes.ToHashSet();
        if (returnTypesUnique.Count != 1)
            throw new Exception(
                $"All returns should only contain one return type, but: {string.Join(", ", returnTypesUnique)}");

        return returnTypesUnique.First();
    }

    private static readonly SymbolDisplayFormat Format =
        new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static IReadOnlyCollection<ITypeSymbol> GetActualReturnTypes(SemanticModel semanticModel,
        MethodDeclarationSyntax method)
    {
        var returnTypes = new List<ITypeSymbol>();

        var returnStatements = method.DescendantNodes()
            .OfType<ReturnStatementSyntax>()
            .Where(returnStatement => !IsInLocalFunction(returnStatement))
            .ToArray();

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

        static bool IsInLocalFunction(SyntaxNode node)
        {
            var current = node.Parent;

            while (current != null)
            {
                // Проверяем, является ли текущий узел локальной функцией
                if (current is LocalFunctionStatementSyntax)
                {
                    return true;
                }

                // Если дошли до метода - это не локальная функция
                if (current is MethodDeclarationSyntax)
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }
    }
}