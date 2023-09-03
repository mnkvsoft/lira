using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Generating;
using SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Matching.String;
using SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Transform;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions;

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

