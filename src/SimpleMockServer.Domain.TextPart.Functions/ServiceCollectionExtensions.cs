using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String;

namespace SimpleMockServer.Domain.TextPart.Functions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctions(this IServiceCollection sc)
    {
        StringMatchPrettyFunctionFactory.AddMatchFunctions(sc);
        GeneratingPrettyFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IGeneratingFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<IBodyExtractFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<IStringMatchFunctionFactory, StringMatchPrettyFunctionFactory>();
    }
}

