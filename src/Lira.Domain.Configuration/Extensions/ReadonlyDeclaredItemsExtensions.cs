// using Lira.Common.Exceptions;
// using Lira.Common.Extensions;
// using Lira.Domain.Configuration.Rules.ValuePatternParsing;
// using Lira.Domain.TextPart;
// using Lira.Domain.TextPart.Impl.Custom;
// using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
// using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
// using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
//
// namespace Lira.Domain.Configuration.Extensions;
//
// static class ReadonlyDeclaredItemsExtensions
// {
//     public static IObjectTextPart Get(this IReadonlyDeclaredItems items, string fullName)
//     {
//         if (fullName.StartsWith(Consts.ControlChars.RuleVariablePrefix))
//             return items.Variables.GetOrThrow(fullName.TrimStart(Consts.ControlChars.RuleVariablePrefix));
//
//         if (fullName.StartsWith(Consts.ControlChars.LocalVariablePrefix))
//             return items.LocalVariables.GetOrThrow(fullName.TrimStart(Consts.ControlChars.LocalVariablePrefix));
//
//         if (fullName.StartsWith(Consts.ControlChars.FunctionPrefix))
//             return items.Functions.GetOrThrow(fullName.TrimStart(Consts.ControlChars.FunctionPrefix));
//
//         throw new Exception($"Unknown declaration '{fullName}'");
//     }
//
//     public static IEnumerable<DeclaredItem> Where(this IReadonlyDeclaredItems items, Predicate<DeclaredItem> predicate)
//     {
//         return items.Variables.Cast<DeclaredItem>().Union(items.Functions).Where(x => predicate(x));
//     }
//
//     public static string GetFullName(this DeclaredItem item)
//     {
//         if (item is RuleVariable)
//             return Consts.ControlChars.RuleVariablePrefix + item.Name;
//
//         if (item is LocalVariable)
//             return Consts.ControlChars.LocalVariablePrefix + item.Name;
//
//         if (item is Function)
//             return Consts.ControlChars.FunctionPrefix + item.Name;
//
//         throw new UnsupportedInstanceType(item);
//     }
// }