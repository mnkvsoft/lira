using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules;

internal static class CreateFunctionResultExtensions
{
    public static T GetFunctionOrThrow<T>(this CreateFunctionResult<T> createFunctionResult, string invoke)
    {
        if (createFunctionResult is CreateFunctionResult<T>.Success success)
            return success.Function;

        var failed = (CreateFunctionResult<T>.Failed)createFunctionResult;

        var nl = Common.Constants.NewLine;

        throw new Exception(
            $"Failed create function from '{invoke}'." + nl +
            "System function not found and attempt compile C# code failed: " + nl +
            failed.Exception);
    }
}