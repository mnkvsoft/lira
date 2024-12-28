using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;

namespace Lira.Domain.Configuration.Extensions;

static class ReadonlyDeclaredItemsExtensions
{
    public static IObjectTextPart Get(this IReadonlyDeclaredItems items, string fullName)
    {
        if (fullName.StartsWith(Consts.ControlChars.VariablePrefix))
            return items.Variables.GetOrThrow(fullName.TrimStart(Consts.ControlChars.VariablePrefix));

        if (fullName.StartsWith(Consts.ControlChars.FunctionPrefix))
            return items.Functions.GetOrThrow(fullName.TrimStart(Consts.ControlChars.FunctionPrefix));

        throw new Exception($"Unknown declaration '{fullName}'");
    }

    public static IEnumerable<DeclaredItem> Where(this IReadonlyDeclaredItems items, Predicate<DeclaredItem> predicate)
    {
        return items.Variables.Cast<DeclaredItem>().Union(items.Functions).Where(x => predicate(x));
    }

    public static string GetFullName(this DeclaredItem item)
    {
        if (item is Variable)
            return Consts.ControlChars.VariablePrefix + item.Name;

        if (item is Function)
            return Consts.ControlChars.FunctionPrefix + item.Name;

        throw new UnsupportedInstanceType(item);
    }
}