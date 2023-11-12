using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using Lira.Domain.Matching.Request.Matchers;
using Microsoft.Extensions.DependencyInjection;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;

public interface IBodyExtractFunctionFactory
{
    IBodyExtractFunction Create(string value);
}

public interface IGeneratingFunctionFactory
{
    bool TryCreate(string value, [MaybeNullWhen(false)] out IObjectTextPart result);
}

internal class GeneratingPrettyFunctionFactory : IGeneratingFunctionFactory, IBodyExtractFunctionFactory
{
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, Func<string, IBodyExtractFunction>> _bodyExtractFunctionsMap;

    public GeneratingPrettyFunctionFactory(IServiceProvider serviceProvider)
    {
        _functionNameToType = new Dictionary<string, Type>();

        _bodyExtractFunctionsMap = new Dictionary<string, Func<string, IBodyExtractFunction>>
        {
            { FunctionName.ExtractBody.All, _ => new AllExtractFunction() },
            { "jpath", arg => new JsonPathExtractFunction().Apply(x => x.SetArgument(arg)) },
            { "xpath", arg => new XPathExtractFunction().Apply(x => x.SetArgument(arg)) },
            { "form", arg => new FormExtractFunction().Apply(x => x.SetArgument(arg)) },
        };

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

    IBodyExtractFunction IBodyExtractFunctionFactory.Create(string value)
    {
        if (value == "all")
            return new AllExtractFunction();

        foreach (var funcName in _bodyExtractFunctionsMap.Keys)
        {
            var funcNameStart = funcName + ":";
            if (value.StartsWith(funcNameStart))
            {
                var arg = value.Replace(funcNameStart, "").Trim();
                var funcFactory = _bodyExtractFunctionsMap[funcName];
                return funcFactory(arg);
            }
        }

        throw new UnknownFunctionException(value);
    }

    bool IGeneratingFunctionFactory.TryCreate(string value, out IObjectTextPart result)
    {
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
        {
            result = null!;
            return false;
        }

        var function = _serviceProvider.GetRequiredFunction(functionType);

        if (function is not IObjectTextPart objectTextPart)
            throw new Exception($"Function {functionType} not implemented {nameof(IObjectTextPart)}");

        function.SetArgumentIfNeed(argument);

        result = objectTextPart;
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
            .Where(t => t.IsAssignableTo(typeof(IObjectTextPart)) && !t.IsAbstract).ToArray();
        return result;
    }
}