using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.TextPart.Impl.CSharp.Compilation;

namespace SimpleMockServer.Domain.TextPart.Impl.CSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCSharp(this IServiceCollection sc)
    {
        return sc.AddScoped<IGeneratingCSharpFactory, GeneratingCSharpFactory>()
            .AddScoped<Compiler>()
            .AddScoped<CompilationStatistic>()
            .AddScoped<PeImagesCache>()
            .AddSingleton<DynamicAssembliesUnloader>();
    }
}
