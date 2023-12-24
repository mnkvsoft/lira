using Microsoft.Extensions.DependencyInjection;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;

namespace Lira.Domain.TextPart.Impl.CSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCSharp(this IServiceCollection sc)
    {
        return sc.AddScoped<IFunctionFactoryCSharp, FunctionFactory>()
            .AddScoped<Compiler>()
            .AddScoped<CompilationStatistic>()
            .AddScoped<PeImagesCache>()
            .AddSingleton<DynamicAssembliesUnloader>();
    }
}
