using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.Functions.Native.Impls.Generating;
using SimpleMockServer.Domain.Functions.Native.Impls.Matching;

namespace SimpleMockServer.Domain.Functions.Native;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNativeFunctions(this IServiceCollection sc)
    {
        return sc
            .AddSingleton<INativeFunctionsFactory, NativeFunctionFactory>()
            .AddSingleton<GeneratingFunctionRoot>()
            .AddSingleton<MatchingMethodsRoot>();
    }
}

