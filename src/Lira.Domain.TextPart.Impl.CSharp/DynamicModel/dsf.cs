// using static Lira.IntegrationTests.fixtures.rules.generating.csharp_blocks._echo;
//
// using System;
// using System.Text;
// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;
//
// using System.Threading.Tasks;
//
// using System.IO;
//
// using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
// using Lira.Domain;
// using Lira.Domain.TextPart;
// using Lira.Domain.TextPart.Impl.CSharp;
//
// using _custom_class_without_namespace;
//
//
//
//
// namespace __DynamicGenerated;
//
// using Lira.Domain.Matching.Request;
//
//
// public sealed class MatchFunction_3bbddff5961a099a0d931fc4855998c2c9d7e82b : DynamicObjectWithDeclaredPartsBase, IMatchFunctionTyped
// {
//     public ReturnType? ValueType => null;
//
//     public MatchFunction_3bbddff5961a099a0d931fc4855998c2c9d7e82b(DependenciesBase dependencies) : base(dependencies)
//     {
//     }
//
//     public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
//     public async Task<bool> IsMatch(RuleExecutingContext __ctx, string? value)
//     {
//         var __variablesWriter = GetVariablesWriter(__ctx, readOnly: false);
//         try
//         {
//             __variablesWriter["$$chain"]= ((await GetDeclaredPart("$$chain", __ctx)))+ ">3";
//             __variablesWriter["$$key"]= value;
//             return true;
//         }
//         catch (Exception e)
//         {
//             var nl = Lira.Common.Constants.NewLine;
//             throw new Exception("An error has occurred while execute code block: " + nl + "$$chain= $$chain+ \">3\";" + nl + "$$key= value;" + nl + "return true;", e);
//         }
//     }
// }