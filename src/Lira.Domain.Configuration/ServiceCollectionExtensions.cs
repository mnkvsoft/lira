using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Variables;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.System;

namespace Lira.Domain.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services)
    {
        services.TryAddTransient<IMemoryCache, MemoryCache>();

        return services
            .AddFunctionsSystem()
            .AddFunctionsCSharp()
            .AddTransient<ConditionMatcherParser>()
            .AddTransient<RequestMatchersParser>()
            .AddTransient<FileSectionDeclaredItemsParser>()
            .AddScoped<ResponseStrategyParser>()
            .AddScoped<ActionsParser>()
            .AddScoped<GeneratingHttpDataParser>()
            .AddScoped<DeclaredItemsLoader>()
            .AddScoped<DeclaredItemsParser>()
            .AddScoped<ITextPartsParser, TextPartsParser>()
            .AddTransient<RuleFileParser>()
            .AddSingleton<ConfigurationLoader>()
            .AddSingleton<IConfigurationLoader>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddSingleton<GuidParser>()
            .AddSingleton<IntParser>()
            .AddSingleton<FloatParser>()
            .AddSingleton<HexParser>()
            
            .AddSingleton<RangesLoader>()
            .AddSingleton<RangesProvider>()
            .AddSingleton<IRangesProvider>(provider => provider.GetRequiredService<RangesProvider>())

            .AddTransient<RulesLoader>()
            .AddSingleton<IRulesProvider>(provider => provider.GetRequiredService<ConfigurationLoader>());
    }
}
