//using System.Diagnostics.CodeAnalysis;
//using SimpleMockServer.Domain.Functions.Pretty.Functions.Extract;
//using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;
//using SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String;
//using SimpleMockServer.Domain.Models.RulesModel.Generating;
//using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;
//using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

//namespace SimpleMockServer.Domain.Functions.Pretty;

//public interface IPrettyFunctionsFactory
//{
//    bool TryCreateGeneratingFunction(string value, [MaybeNullWhen(false)] out IGeneratingFunction function);

//    bool TryCreateStringMatchFunction(string value, [MaybeNullWhen(false)] out IStringMatchFunction function);
//}

//internal class PrettyFunctionsFactory : IPrettyFunctionsFactory
//{
//    private readonly ExtractFunctionsFactory _extractFunctionsFactory;
    
//    private readonly GeneratingPrettyFunctionFactory _generatingFunctionsFactory;

//    private readonly StringMatchPrettyFunctionFactory _stringMatchFunctionsFactory;

//    public PrettyFunctionsFactory(
//        ExtractFunctionsFactory extractFunctionsFactory,
//        StringMatchPrettyFunctionFactory stringMatchFunctionsFactory,
//        GeneratingPrettyFunctionFactory generatingFunctionsFactory)
//    {
//        _extractFunctionsFactory = extractFunctionsFactory;
//        _stringMatchFunctionsFactory = stringMatchFunctionsFactory;
//        _generatingFunctionsFactory = generatingFunctionsFactory;
//    }

//    public bool TryCreateExtractFunction(string value, [MaybeNullWhen(false)] out IBodyExtractFunction function)
//    {
//        return _extractFunctionsFactory.TryCreateForBody(value, out function);
//    }

//    public bool TryCreateGeneratingFunction(string value, [MaybeNullWhen(false)] out IGeneratingFunction function)
//    {
//        bool result = _generatingFunctionsFactory.TryCreate(value, out var generatingFunction);
//        function = generatingFunction;
//        return result;
//    }

//    public bool TryCreateStringMatchFunction(string value, [MaybeNullWhen(false)] out IStringMatchFunction function)
//    {
//        bool result = _stringMatchFunctionsFactory.TryCreate(value, out var matchFunction);
//        function = matchFunction;
//        return result;
//    }
//}
