using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Extract;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Matching;

namespace SimpleMockServer.Domain.Functions.Pretty;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPrettyFunctions(this IServiceCollection sc)
    {
        MatchPrettyFunctionFactory.AddMatchFunctions(sc);
        GeneratingPrettyFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IPrettyFunctionsFactory, PrettyFunctionsFactory>()
            .AddSingleton<ExtractFunctionsFactory>()
            .AddSingleton<GeneratingPrettyFunctionFactory>()
            .AddSingleton<MatchPrettyFunctionFactory>();
    }
}

