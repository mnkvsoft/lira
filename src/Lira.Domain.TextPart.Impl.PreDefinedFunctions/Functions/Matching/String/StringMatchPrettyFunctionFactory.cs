﻿using System.Reflection;
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
            var nameProperty = functionType.GetProperties().SingleOrDefault(x => x.Name == "Name");

            if (nameProperty == null)
                throw new Exception($"Match function '{functionType}' must define static Name property");

            var functionName = (string?)nameProperty.GetValue(null, null);

            if (string.IsNullOrEmpty(functionName))
                throw new Exception("Empty function name in type " + functionType.FullName);

            if (_functionNameToType.TryGetValue(functionName, out var value))
                throw new Exception($"Function with name {functionName} already define in type {value.FullName}");

            _functionNameToType.Add(functionName, functionType);
        }

        _serviceProvider = serviceProvider;
    }

    IStringMatchFunction IStringMatchFunctionFactory.Create(string value)
    {
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
            throw new UnknownFunctionException(value);

        var prettyMatchFunction = (IStringMatchPrettyFunction)_serviceProvider.GetRequiredService(functionType);

        ArgumentFunctionsUtils.SetArgumentIfNeed(prettyMatchFunction, argument, functionName);

        return prettyMatchFunction;
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