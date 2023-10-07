using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
            {FunctionName.ExtractBody.All, _ => new AllExtractFunction()},
            {"jpath", arg => new JsonPathExtractFunction().Apply(x => x.SetArgument(arg))},
            {"xpath", arg => new XPathExtractFunction().Apply(x => x.SetArgument(arg))},
            {"form", arg => new FormExtractFunction().Apply(x => x.SetArgument(arg))},
        };

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

        var function = _serviceProvider.GetRequiredService(functionType) as IObjectTextPart;

        if (function == null)
            throw new Exception("Unknown function type: " + functionType);

        if (function is IWithArgument)
        {
            if (function is not IWithOptionalArgument)
            {
                if(string.IsNullOrEmpty(argument))
                    throw new Exception($"Function '{functionName}' not support arguments");

                SetTypedArgument(function, argument, functionName);
            }

            if (!string.IsNullOrEmpty(argument))
            {
                SetTypedArgument(function, argument, functionName);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(argument))
                throw new Exception($"Function '{functionName}' not support arguments");
        }

        result = function;
        return true;
    }

    private static void SetTypedArgument(IObjectTextPart function, string argument, string functionName)
    {
        if (function is IWithStringArgumentFunction withStringArgument)
        {
            withStringArgument.SetArgument(argument);
        }
        else if (function is IWithIntArgumentFunction withIntArgument)
        {
            if (!int.TryParse(argument, out int intValue))
                throw new Exception($"Function '{functionName}' expected int argument. Current: '{argument}'");

            withIntArgument.SetArgument(intValue);
        }
        else if (function is IWithLongRangeArgumentFunction withIntRangeArgument)
        {
            if (!Interval<long>.TryParse(argument, out var rangeValue))
                throw new Exception($"Function '{functionName}' expected int range argument. Current: '{argument}'");

            withIntRangeArgument.SetArgument(rangeValue);
        }
        else if (function is IWithDecimalRangeArgumentFunction withRangeArgument)
        {
            if (!Interval<decimal>.TryParse(argument, out var rangeValue))
                throw new Exception($"Function '{functionName}' expected float range argument. Current: '{argument}'");

            withRangeArgument.SetArgument(rangeValue);
        }
        else
        {
            throw new Exception($"Function '{functionName}' contains unknown argument");
        }
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
