using System.Diagnostics.CodeAnalysis;
using Lira.Domain.Matching.Request;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain.TextPart.Impl.System;

public interface IFunctionFactorySystem
{
    bool TryCreateBodyExtractFunction(string invoke, [MaybeNullWhen(false)] out IBodyExtractFunction function);

    bool TryCreateGeneratingFunction(string invoke, [MaybeNullWhen(false)] out IObjectTextPart function);

    bool TryCreateTransformFunction(string invoke, [MaybeNullWhen(false)] out ITransformFunction function);

    bool TryCreateMatchFunction(string invoke, [MaybeNullWhen(false)] out IMatchFunctionTyped function);
}