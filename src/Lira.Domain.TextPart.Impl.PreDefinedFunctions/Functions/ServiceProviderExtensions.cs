using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

static class ServiceProviderExtensions
{
    public static FunctionBase GetRequiredFunction(this IServiceProvider provider, Type functionType)
    {
        var function = provider.GetRequiredService(functionType) as FunctionBase;

        if (function == null)
            throw new Exception($"Type '{functionType}' is not {nameof(FunctionBase)}");

        return function;
    }
}