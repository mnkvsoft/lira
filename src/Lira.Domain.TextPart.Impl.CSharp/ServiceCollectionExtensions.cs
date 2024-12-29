﻿using Microsoft.Extensions.DependencyInjection;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;

namespace Lira.Domain.TextPart.Impl.CSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionsCSharp(this IServiceCollection sc)
    {
        return sc.AddScoped<IFunctionFactoryCSharp, FunctionFactory>()
            .AddScoped<Compiler>()
            .AddScoped<CompilationStatistic>()
            .AddScoped<PeImagesCache>()
            .AddSingleton<PeImagesCache.State>()
            .AddSingleton<DynamicAssembliesUnloader>()
            .AddSingleton<ExternalLibsProvider>()
            .AddSingleton<Cache>();
    }
}
