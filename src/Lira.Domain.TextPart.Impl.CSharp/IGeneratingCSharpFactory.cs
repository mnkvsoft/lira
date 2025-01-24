// ReSharper disable RedundantExplicitArrayCreation

using Lira.Domain.Actions;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharp : IDisposable
{
    Task<CreateFunctionResult<IAction>> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    Task<CreateFunctionResult<IObjectTextPart>> TryCreateGeneratingFunction(
        IDeclaredPartsProvider declaredPartsProvider, CodeBlock code);
    Task<CreateFunctionResult<ITransformFunction>> TryCreateTransformFunction(CodeBlock code);
    Task<CreateFunctionResult<IMatchFunctionTyped>> TryCreateMatchFunction(IDeclaredPartsProvider declaredPartsProvider,
        CodeBlock code);
    Task<CreateFunctionResult<IRequestMatcher>> TryCreateRequestMatcher(IDeclaredPartsProvider declaredPartsProvider,
        CodeBlock code);
}