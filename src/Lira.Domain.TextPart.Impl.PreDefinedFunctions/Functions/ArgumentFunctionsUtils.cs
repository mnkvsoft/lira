using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

static class ArgumentFunctionsUtils
{
    public static void SetArgumentIfNeed(object function, string? argument, string functionName)
    {
        if (function is IWithArgument withArgument)
        {
            if (withArgument.ArgumentIsRequired)
            {
                if (string.IsNullOrEmpty(argument))
                    throw new Exception($"Function '{functionName}' argument required");

                SetTypedArgument(withArgument, argument, functionName);
            }

            if (!string.IsNullOrEmpty(argument))
            {
                SetTypedArgument(withArgument, argument, functionName);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(argument))
                throw new Exception($"Function '{functionName}' not support arguments");
        }
    }
    
    private static void SetTypedArgument(IWithArgument function, string argument, string functionName)
    {
        var withArgumentInterface = function.TryGetWithArgumentInterfaceType();

        if (withArgumentInterface != null)
        {
            var argumentType = withArgumentInterface.GetGenericArguments().Single();
            if (!StringConverter.TryConvert(argumentType, argument, out object? result))
                throw new Exception($"Function '{functionName}' expected argument with type: '{argumentType}'. Current: '{argument}'");

            var withArgumentFunction = (IWithArgumentFunction)function;
            withArgumentFunction.SetArgument(result);
            return;
        }

        var withRangeArgumentInterface = function.TryGetWithRangeArgumentInterfaceType();

        if (withRangeArgumentInterface != null)
        {
            var argumentType = withRangeArgumentInterface.GetGenericArguments().Single();
            argument = argument.Trim().TrimStart("[").TrimEnd("]");

            var (fromStr, toStr) = argument.SplitToTwoPartsRequired("-").Trim();

            if (!StringConverter.TryConvert(argumentType, fromStr, out object? objFrom))
                throw new Exception(
                    $"Function '{functionName}' expected range argument with type: '{argumentType}'. Current 'from': '{fromStr}'");

            if (!StringConverter.TryConvert(argumentType, toStr, out object? objTo))
                throw new Exception(
                    $"Function '{functionName}' expected range argument with type: '{argumentType}'. Current 'from': '{toStr}'");

            var withRangeArgumentFunction = (IWithRangeArgumentFunction)function;

            withRangeArgumentFunction.SetArgument(objFrom, objTo);
            return;
        }

        throw new Exception($"Function '{functionName}' contains unknown argument");
    }

    private static Type? TryGetWithArgumentInterfaceType(this IWithArgument function)
    {
        return function.GetType().GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(IWithArgumentFunction<>));
    }

    private static Type? TryGetWithRangeArgumentInterfaceType(this IWithArgument function)
    {
        return function.GetType().GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(IWithRangeArgumentFunction<>));
    }
}