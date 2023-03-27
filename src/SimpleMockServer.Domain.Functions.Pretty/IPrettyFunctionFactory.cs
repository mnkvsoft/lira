using System.Diagnostics.CodeAnalysis;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Extract;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Matching;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty;

public interface IPrettyFunctionsFactory
{
    bool TryCreateExtractFunction(string value, [MaybeNullWhen(false)] out IExtractFunction function);
    bool TryCreateMatchFunction(string value, [MaybeNullWhen(false)] out IMatchFunction function);
    bool TryCreateGeneratingFunction(string value, [MaybeNullWhen(false)] out IGeneratingFunction function);
}

internal class PrettyFunctionsFactory : IPrettyFunctionsFactory
{
    private readonly ExtractFunctionsFactory _extractFunctionsFactory;
    private readonly MatchPrettyFunctionFactory _matchFunctionsFactory;
    private readonly GeneratingPrettyFunctionFactory _generatingFunctionsFactory;

    public PrettyFunctionsFactory(
        ExtractFunctionsFactory extractFunctionsFactory,
        MatchPrettyFunctionFactory matchFunctionsFactory,
        GeneratingPrettyFunctionFactory generatingFunctionsFactory)
    {
        _extractFunctionsFactory = extractFunctionsFactory;
        _matchFunctionsFactory = matchFunctionsFactory;
        _generatingFunctionsFactory = generatingFunctionsFactory;
    }

    public bool TryCreateExtractFunction(string value, [MaybeNullWhen(false)] out IExtractFunction function)
    {
        return _extractFunctionsFactory.TryCreate(value, out function);
    }

    public bool TryCreateGeneratingFunction(string value, [MaybeNullWhen(false)] out IGeneratingFunction function)
    {
        bool result = _generatingFunctionsFactory.TryCreate(value, out var generatingFunction);
        function = generatingFunction;
        return result;
    }

    public bool TryCreateMatchFunction(string value, [MaybeNullWhen(false)] out IMatchFunction function)
    {
        bool result = _matchFunctionsFactory.TryCreate(value, out var matchFunction);
        function = matchFunction;
        return result;
    }
}
