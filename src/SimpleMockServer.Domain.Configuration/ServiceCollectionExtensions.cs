using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleMockServer.Domain.Configuration.DataModel;
using SimpleMockServer.Domain.Configuration.Rules;
using SimpleMockServer.Domain.Configuration.Rules.Parsers;
using SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.TextPart.Functions;

namespace SimpleMockServer.Domain.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainConfiguration(this IServiceCollection services)
    {
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services
           .AddFunctions()
           .AddSingleton<ConditionMatcherParser>()
           .AddSingleton<RequestMatchersParser>()
           .AddSingleton<VariablesParser>()
           .AddSingleton<ResponseWriterParser>()
           .AddSingleton<ExternalCallerParser>()
           .AddSingleton<GeneratingHttpDataParser>()
           .AddSingleton<GlobalVariablesParser>()
           .AddSingleton<GlobalVariableSet>()
           .AddSingleton<ITextPartsParser, TextPartsParser>()
           .AddSingleton<RulesFileParser>()
           .AddSingleton<IConfigurationPathProvider, ConfigurationPathProvider>()
           
           .AddSingleton<DataProvider>()
           .AddSingleton<DataProviderWithState>()
           .AddSingleton<IDataProvider>(provider => provider.GetRequiredService<DataProviderWithState>())
           .AddSingleton<IStatedProvider>(provider => provider.GetRequiredService<DataProviderWithState>())
           
           .AddSingleton<RulesProvider>()
           .AddSingleton<RulesProviderWithState>()
           .AddSingleton<IRulesProvider>(provider => provider.GetRequiredService<RulesProviderWithState>())
           .AddSingleton<IStatedProvider>(provider => provider.GetRequiredService<RulesProviderWithState>());
    }
}
