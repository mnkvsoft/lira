using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;

namespace SimpleMockServer.Domain.Functions.Native.Impls.Generating;

internal record GeneratingFunction : IGeneratingFunction
{
    private readonly IReadOnlyList<MethodCall> _callChain;
    private readonly GeneratingFunctionRoot _methodsRoot;

    public GeneratingFunction(GeneratingFunctionRoot methodsRoot, IReadOnlyList<MethodCall> callChain)
    {
        _callChain = callChain;
        _methodsRoot = methodsRoot;
    }

    public string? Generate(RequestData request)
    {
        return CallChainExecutor.Execute(_methodsRoot, _callChain)?.ToString();
    }
}

