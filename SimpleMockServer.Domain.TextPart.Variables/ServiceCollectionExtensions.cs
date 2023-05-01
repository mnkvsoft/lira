using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.TextPart.Variables.Global;

namespace SimpleMockServer.Domain.TextPart.Functions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVariables(this IServiceCollection sc)
    {
        return sc
            .AddSingleton<IGlobalVariableSet, GlobalVariableSet>();
    }
}

