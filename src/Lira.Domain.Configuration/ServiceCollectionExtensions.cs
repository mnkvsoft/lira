using Lira.Domain.Configuration.DeclarationItems;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.Configuration.RulesStorageStrategies;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.System;
using Microsoft.Extensions.Configuration;

namespace Lira.Domain.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<IMemoryCache, MemoryCache>();

        services.Configure<GitConfig>(configuration);

        return services
            .AddFunctionsSystem()
            .AddFunctionsCSharp()
            .AddTransient<ConditionMatcherParser>()
            .AddTransient<RequestMatchersParser>()

            .AddTransient<RuleFileParser>()

            .AddScoped<OperatorParser>()
            .AddScoped<OperatorPartFactory>()

            .AddScoped<IfOperatorDefinition>()
            .AddScoped<OperatorDefinition>(provider => provider.GetRequiredService<IfOperatorDefinition>())
            .AddScoped<IOperatorHandler, IfHandler>()

            .AddScoped<RandomOperatorDefinition>()
            .AddScoped<OperatorDefinition>(provider => provider.GetRequiredService<RandomOperatorDefinition>())
            .AddScoped<IOperatorHandler, RandomHandler>()

            .AddScoped<RepeatOperatorDefinition>()
            .AddScoped<OperatorDefinition>(provider => provider.GetRequiredService<RepeatOperatorDefinition>())
            .AddScoped<IOperatorHandler, RepeatHandler>()

            .AddScoped<ResponseGenerationHandlerParser>()
            .AddScoped<HandlersParser>()
            .AddScoped<HeadersParser>()
            .AddScoped<GetDelayParser>()
            .AddScoped<DeclaredItemsLoader>()
            .AddScoped<DeclaredItemsLinesParser>()
            .AddScoped<TextPartsParserInternal>()
            .AddScoped<ITextPartsParser, TextPartsParser>()
            .AddScoped<DeclaredItemDraftsParser>()
            .AddScoped<FileSectionDeclaredItemsParser>()

            .AddSingleton<LocalDirectoryRulesStorageStrategy>()
            .AddSingleton<GitRulesStorageStrategy>()

            .AddSingleton<IRulesStorageStrategy>(RulesStorageFactory)
            .AddSingleton<IRulesPathProvider>(p => p.GetRequiredService<IRulesStorageStrategy>())

            .AddSingleton<ConfigurationLoader>()
            .AddSingleton<IConfigurationLoader>(provider => provider.GetRequiredService<ConfigurationLoader>())
            .AddSingleton<IStateRepository, StateRepository>()

            .AddSingleton<GuidParser>()
            .AddSingleton<IntParser>()
            .AddSingleton<PanParser>()
            .AddSingleton<DecParser>()
            .AddSingleton<HexParser>()

            .AddSingleton<ConfigurationReader>()
            .AddSingleton<RangesLoader>()
            .AddScoped<RangesProvider>()
            .AddScoped<IRangesProvider>(provider => provider.GetRequiredService<RangesProvider>())

            .AddTransient<RulesLoader>()
            .AddSingleton<IRulesProvider>(provider => provider.GetRequiredService<ConfigurationLoader>());
    }

    private static IRulesStorageStrategy RulesStorageFactory(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var mode = config.GetValue<string>("RulesStorageMode") ?? "local";

        if (mode.Equals("local", StringComparison.OrdinalIgnoreCase))
            return provider.GetRequiredService<LocalDirectoryRulesStorageStrategy>();

        if (mode.Equals("git", StringComparison.OrdinalIgnoreCase))
            return provider.GetRequiredService<GitRulesStorageStrategy>();

        throw new ArgumentException($"Unknown RulesStorageMode: {mode}");
    }
}
