// using Lira.Common;
// using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;
//
// namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
//
// public class VariableSet : UniqueSet<RuleVariable>
// {
//     public VariableSet(IReadOnlyCollection<RuleVariable> variables) : base(variables)
//     {
//     }
//
//     public VariableSet()
//     {
//     }
//
//     public void TryAddRuntimeVariables(IEnumerable<RuntimeRuleVariable> runtimeVariables)
//     {
//         foreach (var variable in runtimeVariables)
//         {
//             TryAdd(variable);
//         }
//     }
// }