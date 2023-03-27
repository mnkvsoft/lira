using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching;

namespace SimpleMockServer.Domain.Functions.Native;
public interface INativeFunctionsFactory
{
    IGeneratingFunction CreateGeneratingFunction(string callChainString);
    IMatchFunction CreateMatchFunction(string callChainString);
}