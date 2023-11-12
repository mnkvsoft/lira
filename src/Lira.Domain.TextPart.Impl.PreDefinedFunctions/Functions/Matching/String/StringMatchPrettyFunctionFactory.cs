using System.Reflection;
using System.Runtime.Serialization;
using Lira.Domain.Matching.Request;
using Microsoft.Extensions.DependencyInjection;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String;

public interface IStringMatchFunctionFactory
{
    IStringMatchFunction Create(string value);
}

internal class StringMatchPrettyFunctionFactory : IStringMatchFunctionFactory
{
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    public StringMatchPrettyFunctionFactory(IServiceProvider serviceProvider)
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

    IStringMatchFunction IStringMatchFunctionFactory.Create(string value)
    {
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
            throw new UnknownFunctionException(value);

        var function = _serviceProvider.GetRequiredFunction(functionType);

        if (function is not IStringMatchPrettyFunction matchPrettyFunction)
            throw new Exception($"Function {functionType} not implemented {nameof(IStringMatchPrettyFunction)}");
        
        function.SetArgumentIfNeed(argument);

        return matchPrettyFunction;
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
            .Where(t => t.IsAssignableTo(typeof(IStringMatchPrettyFunction)) && !t.IsAbstract).ToArray();
        return result;
    }
}