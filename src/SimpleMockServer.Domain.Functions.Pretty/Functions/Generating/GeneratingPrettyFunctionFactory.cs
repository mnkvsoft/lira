using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;

public interface IBodyExtractFunctionFactory
{
    IBodyExtractFunction Create(string value);
}

public interface IGeneratingFunctionFactory
{
    IGeneratingFunction Create(string value);
}

internal class GeneratingPrettyFunctionFactory : IGeneratingFunctionFactory, IBodyExtractFunctionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, Func<string, IBodyExtractFunction>> _bodyExtractFunctionsMap;

    public GeneratingPrettyFunctionFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _functionNameToType = new Dictionary<string, Type>();

        _bodyExtractFunctionsMap = new Dictionary<string, Func<string, IBodyExtractFunction>>
        {
            {FunctionName.ExtractBody.All, arg => new AllExtractFunction()},
            {"jpath", arg => new JsonPathExtractFunction(_loggerFactory).Apply(x => x.SetArgument(arg))},
            {"xpath", arg => new XPathExtractFunction(_loggerFactory).Apply(x => x.SetArgument(arg))},
            {"form", arg => new FormExtractFunction(_loggerFactory).Apply(x => x.SetArgument(arg))},
        };

        foreach (var functionType in GetMatchFunctionTypes())
        {
            var nameProperty = functionType.GetProperties().SingleOrDefault(x => x.Name == "Name");

            if (nameProperty == null)
                throw new Exception($"Mutch function '{functionType}' must define static Name property");

            string? functionName = (string?)nameProperty.GetValue(null, null);

            if (string.IsNullOrEmpty(functionName))
                throw new Exception("Empty function name in type " + functionType.FullName);

            if (_functionNameToType.ContainsKey(functionName))
                throw new Exception($"Function with name {functionName} already define in type {_functionNameToType[functionName].FullName}");

            _functionNameToType.Add(functionName, functionType);
        }
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    IBodyExtractFunction IBodyExtractFunctionFactory.Create(string value)
    {
        if (value == "all")
            return new AllExtractFunction();

        foreach (var funcName in _bodyExtractFunctionsMap.Keys)
        {
            string funcNameStart = funcName + ":";
            if (value.StartsWith(funcNameStart))
            {
                string arg = value.Replace(funcNameStart, "").Trim();
                Func<string, IBodyExtractFunction> funcFactory = _bodyExtractFunctionsMap[funcName];
                return funcFactory(arg);
            }
        }

        throw new UnknownFunctionException(value);
    }

    IGeneratingFunction IGeneratingFunctionFactory.Create(string value)
    {
        (string functionInvoke, string? format) = value.SplitToTwoParts(" format:").Trim();
        (string functionName, string? argument) = functionInvoke.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out Type? functionType))
            throw new UnknownFunctionException(value);

        var generatingPrettyFunction = (IGeneratingPrettyFunction)_serviceProvider.GetRequiredService(functionType);

        if (argument != null && generatingPrettyFunction is not IWithStringArgumenFunction)
            throw new Exception($"Function '{functionName}' not support arguments");

        if (generatingPrettyFunction is IWithStringArgumenFunction withArgument)
        {
            withArgument.SetArgument(argument!);
        }

        return new GeneratingPrettyFunction(generatingPrettyFunction, format);
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
            .Where(t => t.IsAssignableTo(typeof(IGeneratingPrettyFunction)) && !t.IsAbstract).ToArray();
        return result;
    }
}
