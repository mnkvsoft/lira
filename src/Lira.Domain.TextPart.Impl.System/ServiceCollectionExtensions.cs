using Lira.Domain.TextPart.Impl.System.Functions.Generating;
using Lira.Domain.TextPart.Impl.System.Functions.Matching;
using Lira.Domain.TextPart.Impl.System.Functions.Transform;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain.TextPart.Impl.System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionsSystem(this IServiceCollection sc)
    {
        MatchFunctionFactory.AddMatchFunctions(sc);
        GeneratingFunctionFactory.AddMatchFunctions(sc);

        return sc
            .AddSingleton<IFunctionFactorySystem, FunctionFactorySystem>()
            .AddSingleton<GeneratingFunctionFactory>()
            .AddSingleton<TransformFunctionFactory>()
            .AddSingleton<MatchFunctionFactory>();
    }
}

