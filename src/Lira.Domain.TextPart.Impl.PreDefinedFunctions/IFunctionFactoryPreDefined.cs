using System.Diagnostics.CodeAnalysis;
using Lira.Domain.Matching.Request;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Transform;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions;

public interface IFunctionFactoryPreDefined
{
    bool TryCreateBodyExtractFunction(string invoke, [MaybeNullWhen(false)] out IBodyExtractFunction function);

    bool TryCreateGeneratingFunction(string invoke, [MaybeNullWhen(false)] out IObjectTextPart function);

    bool TryCreateTransformFunction(string invoke, [MaybeNullWhen(false)] out ITransformFunction function);

    bool TryCreateMatchFunction(string invoke, [MaybeNullWhen(false)] out IMatchFunction function);
}


internal class FunctionFactoryPreDefined : IFunctionFactoryPreDefined
{
    private readonly GeneratingFunctionFactory _generatingFunctionFactory;
    private readonly MatchFunctionFactory _matchFunctionFactory;
    private readonly TransformFunctionFactory _transformFunctionFactory;

    public FunctionFactoryPreDefined(GeneratingFunctionFactory generatingFunctionFactory, MatchFunctionFactory matchFunctionFactory, TransformFunctionFactory transformFunctionFactory)
    {
        _generatingFunctionFactory = generatingFunctionFactory;
        _matchFunctionFactory = matchFunctionFactory;
        _transformFunctionFactory = transformFunctionFactory;
    }

    public bool TryCreateBodyExtractFunction(string invoke, [MaybeNullWhen(false)] out IBodyExtractFunction function)
    {
        return _generatingFunctionFactory.TryCreateBodyExtractFunction(invoke, out function);
    }

    public bool TryCreateGeneratingFunction(string invoke, [MaybeNullWhen(false)] out IObjectTextPart function)
    {
        return _generatingFunctionFactory.TryCreateGeneratingFunction(invoke, out function);
    }

    public bool TryCreateMatchFunction(string invoke, [MaybeNullWhen(false)] out IMatchFunction function)
    {
        return _matchFunctionFactory.TryCreate(invoke, out function);
    }

    public bool TryCreateTransformFunction(string invoke, [MaybeNullWhen(false)] out ITransformFunction function)
    {
        return _transformFunctionFactory.TryCreate(invoke, out function);
    }
}