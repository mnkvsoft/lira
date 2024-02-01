using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules;

internal static class CreateFunctionResultExtensions
{
    public static T GetFunctionOrThrow<T>(this CreateFunctionResult<T> createFunctionResult, string invoke, IReadonlyDeclaredItems declaredItems)
    {
        if (createFunctionResult is CreateFunctionResult<T>.Success success)
            return success.Function;

        var failed = (CreateFunctionResult<T>.Failed)createFunctionResult;

        var nl = Common.Constants.NewLine;

        throw new Exception(
            $"Failed dynamic block from:"  + nl +
            "========== begin ==========" + nl + nl +
            invoke + nl + nl +
            "=========== end ===========" + nl +
            "System function, declared function or variable not found." + nl +
            $"Declared functions: {string.Join(", ", declaredItems.Functions.Select(x => Consts.ControlChars.FunctionPrefix + x.Name))}" + nl +
            $"Declared variables: {string.Join(", ", declaredItems.Variables.Select(x => Consts.ControlChars.VariablePrefix + x.Name))}" + nl +
            "Attempt compile C# code failed: " + nl +
            failed.Exception);
    }
}