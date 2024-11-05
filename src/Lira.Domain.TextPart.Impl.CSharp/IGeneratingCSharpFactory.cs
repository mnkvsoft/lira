// ReSharper disable RedundantExplicitArrayCreation

using Lira.Domain.Actions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharp : IDisposable
{
    CreateFunctionResult<IAction> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, string code);
    CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(IDeclaredPartsProvider declaredPartsProvider, string code);
    CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(string code);
    CreateFunctionResult<IMatchFunction> TryCreateMatchFunction(string code);
    CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(string code);
}