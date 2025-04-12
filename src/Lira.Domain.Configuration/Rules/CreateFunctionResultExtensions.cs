using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules;

internal static class CreateFunctionResultExtensions
{
    public static T GetFunctionOrThrow<T>(this CreateFunctionResult<T> createFunctionResult, string invoke, IReadonlyParsingContext context)
    {
        if (createFunctionResult is CreateFunctionResult<T>.Success success)
            return success.Function;

        var failed = (CreateFunctionResult<T>.Failed)createFunctionResult;

        var nl = Environment.NewLine;

        throw new Exception(
            "Failed create dynamic block:" + nl + nl +
            invoke.WrapBeginEnd() +
            "Attempt compile C# code failed. " + failed.Message  + nl + "Code:" + nl + nl +
            failed.Code.WrapBeginEnd() +
            "Context: " + nl +
            context);
    }
}