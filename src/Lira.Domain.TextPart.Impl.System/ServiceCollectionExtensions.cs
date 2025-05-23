using Lira.Domain.TextPart.Impl.System.Functions.Generating;
using Lira.Domain.TextPart.Impl.System.Functions.Matching;
using Lira.Domain.TextPart.Impl.System.Functions.Transform;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain.TextPart.Impl.System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionsSystem(this IServiceCollection sc)
    {
        MatchFunctionFactory.AddFunctions(sc);
        GeneratingFunctionFactory.AddFunctions(sc);

        return sc
            .AddScoped<IFunctionFactorySystem, FunctionFactorySystem>()
            .AddScoped<GeneratingFunctionFactory>()
            .AddScoped<TransformFunctionFactory>()
            .AddScoped<SystemSequence>()
            .AddScoped<MatchFunctionFactory>();
    }
}

