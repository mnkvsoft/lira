using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using Lira.Domain.Matching.Request;
using Microsoft.Extensions.DependencyInjection;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String;

public interface IPreDefinedMatchFunctionFactory
{
    bool TryCreate(string value, [MaybeNullWhen(false)] out IMatchFunction matchFunction);
}

internal class PreDefinedMatchFunctionFactory : IPreDefinedMatchFunctionFactory
{
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    public PreDefinedMatchFunctionFactory(IServiceProvider serviceProvider)
    {
        _functionNameToType = new Dictionary<string, Type>();

        foreach (var functionType in GetMatchFunctionTypes())
        {
            // to avoid looping in service container
            var function = (FunctionBase)FormatterServices.GetUninitializedObject(functionType);

            if (string.IsNullOrEmpty(function.Name))
                throw new Exception("Empty function name in type " + functionType.FullName);

            if (_functionNameToType.TryGetValue(function.Name, out var value))
                throw new Exception($"Function with name {function.Name} already define in type {value.FullName}");

            _functionNameToType.Add(function.Name, functionType);
        }

        _serviceProvider = serviceProvider;
    }

    public bool TryCreate(string value, [MaybeNullWhen(false)] out IMatchFunction matchFunction)
    {
        matchFunction = null;
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
            return false;

        if (!_serviceProvider.TryGetFunction(functionType, out var function))
            return false;
        
        if (function is not IMatchPrettyFunction matchPrettyFunction)
            throw new Exception($"Function {functionType} not implemented {nameof(IMatchPrettyFunction)}");
        
        function.SetArgumentIfNeed(argument);

        matchFunction = matchPrettyFunction;
        return true;
    }

    public static void AddMatchFunctions(IServiceCollection sc)
    {
        foreach (var functionType in GetMatchFunctionTypes())
        {
            sc.Add(new ServiceDescriptor(functionType, functionType, ServiceLifetime.Transient));
        }
    }

    private static IReadOnlyCollection<Type> GetMatchFunctionTypes()
    {
        var result = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMatchPrettyFunction)) && !t.IsAbstract).ToArray();
        return result;
    }
}