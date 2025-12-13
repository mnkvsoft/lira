using Lira.Domain.Configuration.DeclarationItems;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.Parsers;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
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
        // so that each service has its own separate instance
        services.TryAddTransient<IMemoryCache, MemoryCache>();

        services.Configure<GitConfig>(configuration);

        return services
            .AddSingleton<CodeParser>()
            .AddFunctionsSystem()
            .AddFunctionsCSharp()
            .AddTransient<ConditionMatcherParser>()
            .AddTransient<RequestMatchersParser>()

            .AddTransient<RuleFileParser>()

            .AddScoped<OperatorParser>()
            .AddScoped<OperatorPartFactory>()

            .AddOperators()

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
            .AddSingleton<RangesProvider>()
            .AddSingleton<IRangesProvider>(provider => provider.GetRequiredService<RangesProvider>())

            .AddTransient<RulesLoader>()
            .AddSingleton<IRequestHandlerProvider>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddScoped<CustomDictsProvider>()
            .AddScoped<ICustomDictsProvider>(provider => provider.GetRequiredService<CustomDictsProvider>());
    }

    private static IServiceCollection AddOperators(this IServiceCollection services)
    {
        return services
            .AddOperator<IfOperatorDefinition, IfHandler>()
            .AddOperator<RandomOperatorDefinition, RandomHandler>()
            .AddOperator<RepeatOperatorDefinition, RepeatHandler>();
    }

    private static IServiceCollection AddOperator<TOperatorDefinition, TOperatorHandler>(this IServiceCollection services)
        where TOperatorDefinition : OperatorDefinition
        where TOperatorHandler : class, IOperatorHandler
    {
        return services
            .AddScoped<TOperatorDefinition>()
            .AddScoped<OperatorDefinition>(provider => provider.GetRequiredService<TOperatorDefinition>())
            .AddScoped<IOperatorHandler, TOperatorHandler>();
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
