// ReSharper disable RedundantExplicitArrayCreation

using Lira.Domain.Actions;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharp : IDisposable
{
    CreateFunctionResult<IAction> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(CodeBlock code);
    CreateFunctionResult<IMatchFunctionTyped> TryCreateMatchFunction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
}