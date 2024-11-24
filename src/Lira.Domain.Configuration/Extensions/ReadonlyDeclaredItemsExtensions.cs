using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
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

    public static bool Exists(this IReadonlyDeclaredItems items, string fullName)
    {
        if (fullName.StartsWith(Consts.ControlChars.VariablePrefix))
            return items.Variables.Any(v => v.Name == fullName.TrimStart(Consts.ControlChars.VariablePrefix));

        if (fullName.StartsWith(Consts.ControlChars.FunctionPrefix))
            return items.Functions.Any(v => v.Name == fullName.TrimStart(Consts.ControlChars.FunctionPrefix));

        throw new Exception($"Unknown declaration '{fullName}'");
    }
}