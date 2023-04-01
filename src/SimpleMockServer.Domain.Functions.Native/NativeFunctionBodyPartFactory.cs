using SimpleMockServer.Domain.Functions.Native.Impls.Generating;
using SimpleMockServer.Domain.Functions.Native.Impls.Matching;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;

namespace SimpleMockServer.Domain.Functions.Native;

internal sealed class NativeFunctionFactory : INativeFunctionsFactory
{
    private readonly GeneratingFunctionRoot _generatingFunctionRoot;
    private readonly MatchingMethodsRoot _matchingFunctionRoot;

    public NativeFunctionFactory(GeneratingFunctionRoot generatingFunctionRoot, MatchingMethodsRoot matchingFunctionRoot)
    {
        _generatingFunctionRoot = generatingFunctionRoot;
        _matchingFunctionRoot = matchingFunctionRoot;
    }

    public IGeneratingFunction CreateGeneratingFunction(string callChainString)
    {
        var callChain = CallChainParser.Parse(callChainString);
        return new GeneratingFunction(_generatingFunctionRoot, callChain);
    }

    public IStringMatchFunction CreateMatchFunction(string callChainString)
    {
        var callChain = CallChainParser.Parse(callChainString);
        return new MatchFunction(_matchingFunctionRoot, callChain.Single());
    }
}

