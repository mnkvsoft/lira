//using System.Diagnostics.CodeAnalysis;
//using System.Reflection;
//using Microsoft.Extensions.DependencyInjection;
//using SimpleMockServer.Common.Extensions;

//namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Int;

//internal class IntMatchPrettyFunctionFactory
//{
//    private readonly Dictionary<string, Type> _functionNameToType;
//    private readonly IServiceProvider _serviceProvider;

//    public IntMatchPrettyFunctionFactory(IServiceProvider serviceProvider)
//    {
//        _functionNameToType = new Dictionary<string, Type>();

//        foreach (var functionType in GetMatchFunctionTypes())
//        {
//            var nameProperty = functionType.GetProperties().SingleOrDefault(x => x.Name == "Name");

//            if (nameProperty == null)
//                throw new Exception($"Mutch function '{functionType}' must define static Name property");

//            var functionName = (string?)nameProperty.GetValue(null, null);

//            if (string.IsNullOrEmpty(functionName))
//                throw new Exception("Empty function name in type " + functionType.FullName);

//            if (_functionNameToType.ContainsKey(functionName))
//                throw new Exception($"Function with name {functionName} already define in type {_functionNameToType[functionName].FullName}");

//            _functionNameToType.Add(functionName, functionType);
//        }
//        _serviceProvider = serviceProvider;
//    }

//    internal bool TryCreate(string value, [MaybeNullWhen(false)] out IIntMatchPrettyFunction function)
//    {
//        function = null;

//        (var functionName, var argument) = value.SplitToTwoParts(":").Trim();

//        if (!_functionNameToType.TryGetValue(functionName, out var functionType))
//            return false;

//        var prettyMatchFunction = (IIntMatchPrettyFunction)_serviceProvider.GetRequiredService(functionType);

//        if (argument != null && prettyMatchFunction is not IWithIntArgumenFunction)
//            throw new Exception($"Function '{functionName}' not support arguments");

//        if(!int.TryParse(argument, out var number))
//            throw new Exception($"To function '{functionName}' was passed not number argument '{argument}'");

//        if (prettyMatchFunction is IWithIntArgumenFunction withArgument)
//        {
//            withArgument.SetArgument(number);
//        }

//        function = prettyMatchFunction;
//        return true;
//    }

//    public static void AddMatchFunctions(IServiceCollection sc)
//    {
//        foreach (var functionType in GetMatchFunctionTypes())
//        {
//            sc.Add(new ServiceDescriptor(functionType, functionType, ServiceLifetime.Transient));
//        }
//    }

//    private static IReadOnlyCollection<Type> GetMatchFunctionTypes()
//    {
//        var result = Assembly.GetExecutingAssembly().GetTypes()
//            .Where(t => t.IsAssignableTo(typeof(IIntMatchPrettyFunction)) && !t.IsAbstract).ToArray();
//        return result;
//    }
//}


