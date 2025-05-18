// namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;
//
// public class ObjectTextPartWithSaveVariable(IObjectTextPart objectTextPart, Variable variable) : IObjectTextPart
// {
//     public async Task<dynamic?> Get(RuleExecutingContext context)
//     {
//         var value = await objectTextPart.Get(context);
//         variable.SetValue(context, value);
//         return value;
//     }
//
//     public ReturnType? ReturnType => objectTextPart.ReturnType;
// }