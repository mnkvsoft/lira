// ReSharper disable RedundantExplicitArrayCreation

using Lira.Domain.Actions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharp : IDisposable
{
    CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(IDeclaredPartsProvider declaredPartsProvider, string code);
    CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(IDeclaredPartsProvider declaredPartsProvider, string code);
    CreateFunctionResult<IMatchFunction> TryCreateMatchFunction(IDeclaredPartsProvider declaredPartsProvider, string code);
    CreateFunctionResult<IAction> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, string code);
}