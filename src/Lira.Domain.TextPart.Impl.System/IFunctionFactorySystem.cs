using System.Diagnostics.CodeAnalysis;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Impl.Custom;

namespace Lira.Domain.TextPart.Impl.System;

public record SystemFunctionContext(IDeclaredItemsReadonlyProvider DeclaredItemsProvider);

public interface IFunctionFactorySystem
{
    bool TryCreateBodyExtractFunction(string invoke, [MaybeNullWhen(false)] out IBodyExtractFunction function);

    bool TryCreateGeneratingFunction(string invoke, SystemFunctionContext ctx, [MaybeNullWhen(false)] out IObjectTextPart function);

    bool TryCreateTransformFunction(string invoke, [MaybeNullWhen(false)] out ITransformFunction function);

    bool TryCreateMatchFunction(string invoke, SystemFunctionContext ctx, [MaybeNullWhen(false)] out IMatchFunctionTyped function);
}