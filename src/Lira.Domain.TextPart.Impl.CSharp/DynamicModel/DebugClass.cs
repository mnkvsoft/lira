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
// public sealed class GeneratingFunction_821a4dc57986f26eabcd319d0421f66e650c4618 : DynamicObjectBaseGenerate, IObjectTextPart
// {
//     ReturnType? IObjectTextPart.ReturnType => null;
//
//     public GeneratingFunction_821a4dc57986f26eabcd319d0421f66e650c4618(DependenciesBase dependencies) : base(dependencies)
//     {
//     }
//
//     public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext ctx)
//     {
//         yield return await GetInternal(ctx);
//     }
//
//     private async ValueTask<dynamic?> GetInternal(RuleExecutingContext __ctx)
//     {
//         var req = new RequestModel(__ctx.RequestContext.RequestData);
//         var __variablesWriter = GetVariablesWriter(__ctx, readOnly: false);
//
//         try
//             {
//                 var __result = ((await GetDeclaredPart("#duration.day", __ctx)))+ TimeSpan.FromMinutes(1);
// ; return __result;
//             }
//             catch (Exception e)
//             {
//                 var nl = Lira.Common.Constants.NewLine;
//                 throw new Exception("An error has occurred while execute code block: " + nl + "#duration.day+ TimeSpan.FromMinutes(1)", e);
//             }
//
//         IAsyncEnumerable<dynamic?> repeat(IObjectTextPart part, string separator = ",", int? count = null, int? from = null, int? to = null)
//         {
//             int cnt;
//             if(count != null)
//                 cnt = count.Value;
//             else if(from != null)
//                 cnt = Random.Shared.Next(from.Value, to.Value + 1);
//             else
//                 cnt = Random.Shared.Next(3, 9);
//             return Repeat(__ctx, part, separator, cnt);
//         }
//     }
// }