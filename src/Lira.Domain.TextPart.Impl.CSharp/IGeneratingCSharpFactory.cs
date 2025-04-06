// ReSharper disable RedundantExplicitArrayCreation

using System.Collections.Immutable;
using Lira.Domain.Handling.Actions;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharpFactory
{
    Task<IFunctionFactoryCSharp> Get();
    void Init(IImmutableList<string> fileLines);
}

public interface IFunctionFactoryCSharp
{
    CreateFunctionResult<IAction> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<IMatchFunctionTyped> TryCreateMatchFunction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);

    CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(CodeBlock code);
}