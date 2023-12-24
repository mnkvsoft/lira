using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Lira.Domain.TextPart.Impl.System.Functions;

static class ServiceProviderExtensions
{
    public static FunctionBase GetRequiredFunction(this IServiceProvider provider, Type functionType)
    {
        var function = provider.GetRequiredService(functionType) as FunctionBase;

        if (function == null)
            throw new Exception($"Type '{functionType}' is not {nameof(FunctionBase)}");

        return function;
    }
    
    public static bool TryGetFunction(this IServiceProvider provider, Type functionType, [MaybeNullWhen(false)] out FunctionBase function)
    {
        function = null;
        var temp = provider.GetService(functionType);

        if (temp == null)
            return false;

        function = temp as FunctionBase;

        if (function == null)
            throw new Exception($"Type '{functionType}' is not {nameof(FunctionBase)}");
        
        return true;
    }
}