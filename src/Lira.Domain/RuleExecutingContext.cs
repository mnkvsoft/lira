using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain;

public record RuleExecutingContext(RequestData RequestData)
{
    public IDictionary<Type, object> Items { get; } = new Dictionary<Type, object>();
}

static class RuleExecutingContextExtensions
{
    public static bool TryGetItem<TValue>(this RuleExecutingContext ctx,
        Type key,
        [MaybeNullWhen(false)] out TValue result)
    {
        result = default;
        if (ctx.Items.TryGetValue(key, out object? val))
        {
            result = (TValue)val;
            return true;
        }

        return false;
    }
}