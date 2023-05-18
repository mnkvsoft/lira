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
            .AddSingleton<ConditionMatcherParser>()
            .AddSingleton<RequestMatchersParser>()
            .AddSingleton<FileSectionVariablesParser>()
            .AddSingleton<ResponseWriterParser>()
            .AddSingleton<ExternalCallerParser>()
            .AddSingleton<GeneratingHttpDataParser>()
            .AddSingleton<GlobalVariablesParser>()
            .AddSingleton<VariablesParser>()
            .AddSingleton<ITextPartsParser, TextPartsParser>()
            .AddSingleton<RuleFileParser>()
            .AddSingleton<ConfigurationLoader>()
            .AddSingleton<IConfigurationLoader>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddSingleton<DataLoader>()
            .AddSingleton<IDataProvider>(provider => provider.GetRequiredService<ConfigurationLoader>())

            .AddSingleton<RulesLoader>()
            .AddSingleton<IRulesProvider>(provider => provider.GetRequiredService<ConfigurationLoader>());
    }
}
