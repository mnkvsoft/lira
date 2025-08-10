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

            .AddTransient<RuleFileParser>()

            .AddScoped<OperatorParser>()
            .AddScoped<OperatorPartFactory>()

            .AddScoped<OperatorDefinition, RandomOperatorDefinition>()
            .AddScoped<IOperatorHandler, RandomHandler>()

            .AddScoped<OperatorDefinition, RepeatOperatorDefinition>()
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
}
