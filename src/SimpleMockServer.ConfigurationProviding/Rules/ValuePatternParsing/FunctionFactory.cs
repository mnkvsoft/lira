using SimpleMockServer.Domain.Functions.Native;
using SimpleMockServer.Domain.Functions.Pretty;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

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

    public IMatchFunction CreateMatchFunction(string functionInvoke)
    {
        if (_prettyFunctionFactory.TryCreateMatchFunction(functionInvoke, out var function))
            return function;

        return _nativeFunctionBodyPartFactory.CreateMatchFunction(functionInvoke);
    }

    public IExtractFunction CreateExtractFunction(string functionInvoke)
    {
        if (!_prettyFunctionFactory.TryCreateExtractFunction(functionInvoke, out var function))
            throw new Exception($"Cannot create function invoke '{functionInvoke}'");

        return function;
    }
}
