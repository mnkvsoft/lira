using System.Reflection;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;

namespace SimpleMockServer.Domain.Functions.Native.Impls.Matching;

internal class MatchFunction : IStringMatchFunction
{
    private readonly MatchingMethodsRoot _matchingMethods;
    private readonly object[] _args;
    private readonly MethodInfo _methodInfo;

    public MatchFunction(MatchingMethodsRoot matchingMethods, MethodCall methodCall)
    {
        List<string> allArgs = new List<string>(methodCall.Argumens.Count + 1);
        allArgs.Add("test_matching_value");
        allArgs.AddRange(methodCall.Argumens);
        MethodCall originalCall = new MethodCall(methodCall.Name, allArgs);

        MethodInfo method = CallChainExecutor.GetMethodInfo(matchingMethods.GetType(), originalCall);
        if (method.ReturnType != typeof(bool))
            throw new Exception($"Method {method.Name} must return bool");

        var args = CallChainExecutor.GetMethodParameters(method, allArgs);

        try
        {
            method.Invoke(matchingMethods, args);
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while creating method info for " + methodCall, e);
        }

        _matchingMethods = matchingMethods;

        // first arg contains test value, remove it
        _args = new object[args.Length - 1];
        Array.Copy(args, 1, _args, 0, _args.Length);

        _methodInfo = method;
    }

    public bool IsMatch(string? value)
    {
        object?[] allArgs = new object[_args.Length + 1];
        allArgs[0] = value;
        Array.Copy(_args, 0, allArgs, 1, _args.Length);

        object? isMatch = _methodInfo.Invoke(_matchingMethods, allArgs);
        return (bool)isMatch!;
    }
}