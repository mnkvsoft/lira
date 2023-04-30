using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String;

namespace SimpleMockServer.Domain.Functions.Pretty;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPrettyFunctions(this IServiceCollection sc)
    {
        StringMatchPrettyFunctionFactory.AddMatchFunctions(sc);
        GeneratingPrettyFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IGeneratingFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<IBodyExtractFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<IStringMatchFunctionFactory, StringMatchPrettyFunctionFactory>();
    }
}

