using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.System.Functions.Functions.Matching.String;
using SimpleMockServer.Domain.TextPart.System.Functions.Functions.Transform;

namespace SimpleMockServer.Domain.TextPart.System.Functions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctions(this IServiceCollection sc)
    {
        StringMatchPrettyFunctionFactory.AddMatchFunctions(sc);
        GeneratingPrettyFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IGeneratingFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<IBodyExtractFunctionFactory, GeneratingPrettyFunctionFactory>()
            .AddSingleton<ITransformFunctionFactory, TransformFunctionFactory>()
            .AddSingleton<IStringMatchFunctionFactory, StringMatchPrettyFunctionFactory>();
    }
}

