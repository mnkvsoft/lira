using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions;

static class FunctionBaseExtensions
{
    public static void SetArgumentIfNeed(this FunctionBase function, string? argument)
    {
        if (function is IWithArgument withArgument)
        {
            if (withArgument.ArgumentIsRequired)
            {
                if (string.IsNullOrEmpty(argument))
                    throw new Exception($"Function '{function.Name}' argument required");

                SetTypedArgument(withArgument, argument, function.Name);
            }

            if (!string.IsNullOrEmpty(argument))
            {
                SetTypedArgument(withArgument, argument, function.Name);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(argument))
                throw new Exception($"Function '{function.Name}' not support arguments");
        }
    }

    public static void SetContextIfNeed(this FunctionBase function, SystemFunctionContext context)
    {
        if (function is IWithContext withContext)
        {
            withContext.SetContext(context);
        }
    }

    private static void SetTypedArgument(IWithArgument function, string argument, string functionName)
    {
        var withArgumentInterface = function.TryGetWithArgumentInterfaceType();

        if (withArgumentInterface != null)
        {
            var argumentType = withArgumentInterface.GetGenericArguments().Single();

            object result = GetArgument(argument, functionName, argumentType);

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

    private static object GetArgument(string argument, string functionName, Type argumentType)
    {
        if (argumentType.BaseType == typeof(Array))
        {
            string[] arrayStr = argument.Split(",");
            var elementType = argumentType.GetElementType()!;
            var array = Array.CreateInstance(elementType, arrayStr.Length);
            for (int i = 0; i < arrayStr.Length; i++)
            {
                string str = arrayStr[i].Trim();
                if (!StringConverter.TryConvert(elementType, str, out object? obj))
                    throw new Exception($"Function '{functionName}' expected argument with type: '{elementType}'. Current: '{str}'");
                array.SetValue(obj, i);
            }

            return array;
        }

        if (!StringConverter.TryConvert(argumentType, argument, out object? result))
            throw new Exception($"Function '{functionName}' expected argument with type: '{argumentType}'. Current: '{argument}'");
        return result;
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