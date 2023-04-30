using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.ConfigurationProviding.DataModel;
using SimpleMockServer.ConfigurationProviding.Rules;
using SimpleMockServer.ConfigurationProviding.Rules.Parsers;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Functions.Pretty;
using SimpleMockServer.Domain.Models.DataModel;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;

namespace SimpleMockServer.ConfigurationProviding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services
           .AddPrettyFunctions()
           .AddSingleton<IDataProvider, DataProvider>()
           .AddSingleton<ConditionMatcherParser>()
           .AddSingleton<RequestMatchersParser>()
           .AddSingleton<VariablesParser>()
           .AddSingleton<ResponseWriterParser>()
           .AddSingleton<ExternalCallerParser>()
           .AddSingleton<GeneratingHttpDataParser>()
           .AddSingleton<ITextPartsParser, TextPartsParser>()
           .AddSingleton<RulesFileParser>()
           .AddSingleton<IMemoryCache, MemoryCache>()
           .AddSingleton<IRequestStatisticStorage, RequestStatisticStorage>()
           .AddSingleton<IRulesProvider, RulesProvider>();
    }
}
