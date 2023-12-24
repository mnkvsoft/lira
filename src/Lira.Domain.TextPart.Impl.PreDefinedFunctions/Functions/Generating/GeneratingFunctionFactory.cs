using System.Reflection;
using System.Runtime.Serialization;
using Lira.Domain.Matching.Request.Matchers;
using Microsoft.Extensions.DependencyInjection;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;
using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;

internal class GeneratingFunctionFactory
{
    private readonly Dictionary<string, Type> _functionNameToType;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, Func<string, IBodyExtractFunction>> _bodyExtractFunctionsMap;

    public GeneratingFunctionFactory(IServiceProvider serviceProvider)
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

    public bool TryCreateBodyExtractFunction(string value, [MaybeNullWhen(false)] out IBodyExtractFunction function)
    {
        function = null;
        if (value == "all")
        {
            function = new AllExtractFunction();
            return true;
        }

        var (functionName, arg) = value.SplitToTwoPartsRequired(":").Trim();

        if (_bodyExtractFunctionsMap.TryGetValue(functionName, out var funcFactory))
        {
            function = funcFactory(arg);
            return true;
        }

        return false;
    }

    public bool TryCreateGeneratingFunction(string value, [MaybeNullWhen(false)] out IObjectTextPart function)
    {
        function = null;
        var (functionName, argument) = value.SplitToTwoParts(":").Trim();

        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
            return false;

        var functionTemp = _serviceProvider.GetRequiredFunction(functionType);

        if (functionTemp is not IObjectTextPart objectTextPart)
            throw new Exception($"Function {functionType} not implemented {nameof(IObjectTextPart)}");

        functionTemp.SetArgumentIfNeed(argument);

        function = objectTextPart;
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