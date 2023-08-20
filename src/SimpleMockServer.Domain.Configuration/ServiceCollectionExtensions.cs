using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleMockServer.Domain.Configuration.DataModel;
using SimpleMockServer.Domain.Configuration.Rules;
using SimpleMockServer.Domain.Configuration.Rules.Parsers;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Configuration.Variables;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.TextPart.CSharp;
using SimpleMockServer.Domain.TextPart.Functions;

namespace SimpleMockServer.Domain.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services)
    {
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services
            .AddFunctions()
            .AddCSharp()
            .AddTransient<ConditionMatcherParser>()
            .AddTransient<RequestMatchersParser>()
            .AddTransient<FileSectionVariablesParser>()
            .AddScoped<ResponseWriterParser>()
            .AddScoped<ExternalCallerParser>()
            .AddScoped<GeneratingHttpDataParser>()
            .AddScoped<GlobalVariablesParser>()
            .AddScoped<VariablesParser>()
            .AddScoped<ITextPartsParser, TextPartsParser>()
            .AddTransient<RuleFileParser>()
            .AddSingleton<ConfigurationLoader>()
            .AddSingleton<IConfigurationLoader>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddSingleton<GuidParser>()
            .AddSingleton<IntParser>()
            .AddSingleton<FloatParser>()
            .AddSingleton<HexParser>()
            
            .AddSingleton<DataLoader>()
            .AddSingleton<IDataProvider>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddTransient<RulesLoader>()
            .AddSingleton<IRulesProvider>(provider => provider.GetRequiredService<ConfigurationLoader>());
    }
}
