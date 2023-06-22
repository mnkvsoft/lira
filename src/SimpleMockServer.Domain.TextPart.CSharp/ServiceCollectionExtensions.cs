using Microsoft.Extensions.DependencyInjection;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCSharp(this IServiceCollection sc)
    {
        return sc.AddScoped<IGeneratingCSharpFactory, GeneratingCSharpFactory>()
                    .AddSingleton<DynamicAssembliesUploader>();
    }
}

