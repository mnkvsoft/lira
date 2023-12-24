using Microsoft.Extensions.DependencyInjection;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Transform;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctions(this IServiceCollection sc)
    {
        MatchFunctionFactory.AddMatchFunctions(sc);
        GeneratingFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IFunctionFactoryPreDefined, FunctionFactoryPreDefined>()
            .AddSingleton<GeneratingFunctionFactory>()
            .AddSingleton<TransformFunctionFactory>()
            .AddSingleton<MatchFunctionFactory>();
    }
}

