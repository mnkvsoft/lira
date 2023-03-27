using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.ConfigurationProviding.DataModel;
using SimpleMockServer.ConfigurationProviding.Rules;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Functions.Native;
using SimpleMockServer.Domain.Functions.Pretty;
using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.ConfigurationProviding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services
           .AddNativeFunctions()
           .AddPrettyFunctions()
           .AddSingleton<IDataProvider, DataProvider>()
           .AddSingleton<RulesFileParser>()
           .AddSingleton<FunctionFactory>()
           .AddSingleton<IRulesProvider, RulesProvider>();
    }
}