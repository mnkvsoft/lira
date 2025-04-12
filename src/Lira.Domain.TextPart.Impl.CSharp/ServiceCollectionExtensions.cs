using Microsoft.Extensions.DependencyInjection;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading.Nuget;

namespace Lira.Domain.TextPart.Impl.CSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionsCSharp(this IServiceCollection sc)
    {
        return sc
            .AddTransient<FunctionFactory.Dependencies>()
            .AddTransient<CsFilesCompiler.Dependencies>()

            .AddScoped<IFunctionFactoryCSharpFactory, FunctionFactoryCSharpFactory>()
            .AddScoped<Compiler>()
            .AddScoped<CompilationStatistic>()
            .AddScoped<AssembliesLoader>()
            .AddScoped<PeImagesCache>()
            .AddScoped<Namer>()

            .AddSingleton<PeImagesCache.State>()
            .AddSingleton<Namer.State>()
            .AddSingleton<DynamicAssembliesUnloader>()
            .AddSingleton<NugetLibsProvider>()
            .AddSingleton<ExtLibsProvider>()
            .AddSingleton<Cache>();
    }
}
