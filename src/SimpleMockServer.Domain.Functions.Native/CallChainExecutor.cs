using System.ComponentModel;
using ArgValidation;
using System.Reflection;

namespace SimpleMockServer.Domain.Functions.Native;

internal static class CallChainExecutor
{
    public static object? Execute(object root, IReadOnlyList<MethodCall> callChain)
    {
        Arg.NotEmpty(callChain, nameof(callChain));

        object currentObj = root;

        foreach (MethodCall call in callChain)
        {
            object? obj = CallMethod(currentObj, call);

            if (obj == null)
                throw new Exception($"Method call {call} return null");

            currentObj = obj;

        }

        return currentObj;
    }

    private static object? CallMethod(object root, MethodCall callingMethod)
    {
        MethodInfo method = GetMethodInfo(root.GetType(), callingMethod);
        object[] pars = GetMethodParameters(method, callingMethod.Argumens);
        object? result = method.Invoke(root, pars.ToArray());
        return result;
    }

    public static MethodInfo GetMethodInfo(Type type, MethodCall callingMethod)
    {
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

        methods = methods.Where(m =>
                            m.Name == callingMethod.Name &&
                            m.GetParameters().Length == callingMethod.Argumens.Count)
                        .ToArray();

        if (methods.Length == 0)
            throw new Exception("Not found method " + callingMethod);

        if (methods.Length > 1)
            throw new Exception("More than one method found for " + callingMethod);

        var method = methods.Single();

        return method;
    }

    public static object[] GetMethodParameters(MethodInfo method, IReadOnlyList<string> argumens)
    {
        var parameters = method.GetParameters();
        var pars = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var par = parameters[i];
            var parType = par.ParameterType;

            TypeConverter typeConverter = TypeDescriptor.GetConverter(parType);
            object? propValue = typeConverter.ConvertFromString(argumens[i]);
            pars[i] = propValue!;
        }

        return pars;
    }
}