using SimpleMockServer.Domain.Functions.Native;
using SimpleMockServer.Domain.Functions.Pretty;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;

internal sealed class FunctionFactory
{
    private readonly INativeFunctionsFactory _nativeFunctionBodyPartFactory;
    private readonly IPrettyFunctionsFactory _prettyFunctionFactory;

    public FunctionFactory(INativeFunctionsFactory nativeFunctionBodyPartFactory, IPrettyFunctionsFactory prettyFunctionFactory)
    {
        _nativeFunctionBodyPartFactory = nativeFunctionBodyPartFactory;
        _prettyFunctionFactory = prettyFunctionFactory;
    }

    public IGeneratingFunction CreateGeneratingFunction(string functionInvoke)
    {
        if (_prettyFunctionFactory.TryCreateGeneratingFunction(functionInvoke, out var function))
            return function;

        return _nativeFunctionBodyPartFactory.CreateGeneratingFunction(functionInvoke);
    }

    public IStringMatchFunction CreateStringMatchFunction(string functionInvoke)
    {
        if (_prettyFunctionFactory.TryCreateStringMatchFunction(functionInvoke, out var function))
            return function;

        return _nativeFunctionBodyPartFactory.CreateMatchFunction(functionInvoke);
    }

    //public IIntMatchFunction CreateIntMatchFunction(string functionInvoke)
    //{
    //    if (!_prettyFunctionFactory.TryCreateIntMatchFunction(functionInvoke, out var function))
    //        throw new Exception($"Cannot create function invoke '{functionInvoke}'");

    //    return function;
    //}

    public IExtractFunction CreateExtractFunction(string functionInvoke)
    {
        if (!_prettyFunctionFactory.TryCreateExtractFunction(functionInvoke, out var function))
            throw new Exception($"Cannot create function invoke '{functionInvoke}'");

        return function;
    }
}
