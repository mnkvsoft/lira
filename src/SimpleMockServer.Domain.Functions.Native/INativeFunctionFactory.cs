using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request;

namespace SimpleMockServer.Domain.Functions.Native;
public interface INativeFunctionsFactory
{
    IGeneratingFunction CreateGeneratingFunction(string callChainString);
    IStringMatchFunction CreateMatchFunction(string callChainString);
}