using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lira.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching;

internal class MatchFunctionFactory
{
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    public MatchFunctionFactory(IServiceProvider serviceProvider)
    {
        _functionNameToType = new Dictionary<string, Type>();

        foreach (var functionType in GetMatchFunctionTypes())
        {
            // to avoid looping in service container
            var function = (FunctionBase)RuntimeHelpers.GetUninitializedObject(functionType);

            if (string.IsNullOrEmpty(function.Name))
                throw new Exception("Empty function name in type " + functionType.FullName);

            if (_functionNameToType.TryGetValue(function.Name, out var value))
                throw new Exception($"Function with name {function.Name} already define in type {value.FullName}");

            _functionNameToType.Add(function.Name, functionType);
        }

        _serviceProvider = serviceProvider;
    }

    public bool TryCreate(string value, SystemFunctionContext ctx, [MaybeNullWhen(false)] out IMatchFunctionTyped matchFunction)
    {
        matchFunction = null;
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
            return false;

        if (!_serviceProvider.TryGetFunction(functionType, out var function))
            return false;

        if (function is not IMatchFunctionTyped matchPrettyFunction)
            throw new Exception($"Function {functionType} not implemented {nameof(IMatchFunctionTyped)}");

        function.SetContextIfNeed(ctx);
        function.SetArgumentIfNeed(argument);

        matchFunction = matchPrettyFunction;
        return true;
    }

    public static void AddFunctions(IServiceCollection sc)
    {
        foreach (var functionType in GetMatchFunctionTypes())
        {
            sc.Add(new ServiceDescriptor(functionType, functionType, ServiceLifetime.Transient));
        }
    }

    private static IReadOnlyCollection<Type> GetMatchFunctionTypes()
    {
        var result = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMatchFunctionTyped)) && !t.IsAbstract).ToArray();
        return result;
    }
}